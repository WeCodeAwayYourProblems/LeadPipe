using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace LeadPipe.Infrastructure.Service;

internal record YellerFetchResult(List<string> Raw, int Errors, string FinalId);
internal class YellerClientService : IYellerService
{
    #region Ctor and Private Fields

    private readonly IHttpClientFactory _factory;
    private readonly IYellerSettings _settings;
    private readonly HttpClient _client;
    private readonly IDtoToVo<YellerDto, Plumbing> _dtoToVo;
    private readonly ILogger _logger;
    private readonly ISyncStateRepository _sync;
    private readonly SemaphoreSlim _throttle;
    private const int errorLimit = 5;

    public YellerClientService(
        IHttpClientFactory factory,
        IYellerSettings settings,
        IDtoToVo<YellerDto, Plumbing> dtoToVo,
        ILogger<YellerClientService> logger,
        ISyncStateRepository sync
        )
    {
        _factory = factory;
        _settings = settings;
        _client = _factory.CreateClient(_settings.YellerGetterName!);
        _dtoToVo = dtoToVo;
        _logger = logger;
        _throttle = new SemaphoreSlim(_settings.YellerConcurrentMax);
        _sync = sync;
    }

    #endregion

    const int limit = 20;
    public async Task<Result<List<YellerDto>>> GetAllAsync(bool refresh)
    {
        if (_settings.YellerBellerId is null)
            return Result.Failure<List<YellerDto>>($"{nameof(_settings.YellerBellerId)} list is null");

        string[] yellerIds = _settings.YellerBellerId;
        if (yellerIds.Length == 0)
            return Result.Failure<List<YellerDto>>($"{nameof(_settings.YellerBellerId)} list is empty");

        List<string> allRaw = [];
        int allErrors = 0;
        string process = "Id retrieval";
        Dictionary<string, string> finalIds = [];
        foreach (string yellerId in yellerIds)
        {
            string endpoint = $"{_settings.YellerPrelimEndpoint1}{yellerId}{_settings.YellerPrelimEndpoint2}?limit={limit}";

            // Retrieve sync state id
            // If the syncStateId is empty, then we will retrieve all data
            Result<SyncStateEntity> syncState = 
                Result.Failure<SyncStateEntity>("Still Confirming how codes work on this api");
                //await _sync.GetByIdAsync(BusinessId.From(yellerId));
            string syncStateId = syncState.IsSuccess && refresh
                ? syncState.Value.BusinessId.ToString()
                : string.Empty;

            YellerFetchResult data = await GetData(yellerId, process, endpoint, syncStateId);

            // Aggregate raw and errors
            allRaw.AddRange(data.Raw);
            allErrors += data.Errors;

            if (!string.IsNullOrWhiteSpace(data.FinalId))
                finalIds[yellerId] = data.FinalId;
        }

        // Distinct raw
        allRaw = [.. allRaw.Distinct()];

        // finalId is an opaque, API-defined cursor (alphanumeric).
        // We intentionally persist the first cursor returned by the API,
        // not a computed max, because ordering semantics are undocumented.
        List<SyncStateEntity> states = [];
        foreach (var bId in finalIds.Keys)
        {
            SyncStateEntity state = new()
            {
                BusinessId = BusinessId.From(bId),
                LastProcessedId = finalIds[bId],
                LastSyncUtc = DateTime.UtcNow,
                UnixLastSyncUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            states.Add(state);
        }
        Result<List<SyncStateEntity>> synced = await _sync.UpsertRangeAsync(states);
        if (synced.IsFailure)
        {
            _logger.LogError(
                "Failed to sync. The next call to this api will have data that overlaps with existing persisted data. Error {Error}",
                synced.Error);
        }

        // Check that raw is not empty
        if (allRaw.Count == 0)
        {
            _logger.LogError(
                "Failed prelim retrieval. Errors: {Errors}. Process: {Process}",
                allErrors, process);

            return Result.Failure<List<YellerDto>>("Failed to retrieve data from API.");
        }

        // Hydrate dtos with data by fetching data in all ids
        Result<List<YellerDto>> dtoResult = await GetDto(allRaw);

        return dtoResult;
    }

    private async Task<YellerFetchResult> GetData(string yellerId, string process, string endpoint, string syncStateId)
    {
        List<string> raw = [];
        int errors = 0;
        string finalSyncId = "";
        int callCount = 0;
        while (true)
        {
            if (errors >= errorLimit)
            {
                _logger.LogWarning(
                    "Reached error limit {ErrorLimit}. Retrieved: {Retrieved}. Process: {Process}",
                    errorLimit, raw.Count, process);
                break;
            }

            try
            {
                HttpResponseMessage response = await _client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    errors++;
                    _logger.LogError(
                        "Response failure ({Reason}). Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                        response.ReasonPhrase, errors, errorLimit, raw.Count, process);

                    if (response.ReasonPhrase is not null && response.ReasonPhrase.Contains("Unauthorized"))
                        break;

                    continue;
                }

                YellerHelperDto? value = await response.Content.ReadFromJsonAsync<YellerHelperDto>();

                if (value?.lead_ids == null || value.lead_ids.Length == 0)
                {
                    errors++;
                    _logger.LogWarning(
                        "Null or invalid prelim response. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                        errors, errorLimit, raw.Count, process);

                    continue;
                }

                // Ids are retrieved in a chronological stack
                // Therefore, the sync state id must be the most recent chronologically,
                // which is the first id of the first call
                if (callCount == 0)
                    finalSyncId = value.lead_ids[0];
                callCount++;

                string nextId = value.lead_ids[^1];
                endpoint = $"{_settings.YellerPrelimEndpoint1}{yellerId}{_settings.YellerPrelimEndpoint2}?limit={limit}&{_settings.YellerPrelimId}={nextId}";

                // If the syncstateid is null or empty, then we retrieve all the way to the end
                // Otherwise, we need to trim any id including and after the syncstateid
                if (!string.IsNullOrEmpty(syncStateId))
                {
                    // if string[] does not contain syncStateId, index == -1
                    int index = Array.IndexOf(value.lead_ids, syncStateId);
                    if (index >= 0)
                    {
                        raw.AddRange(value.lead_ids[..index]);
                        break;
                    }
                }

                raw.AddRange(value.lead_ids);

                if (!value.has_more)
                    break;

            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(
                    ex,
                    "Exception in prelim stage. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Exception Message: {Message}",
                    errors, errorLimit, raw.Count, process, ex.Message);
            }
        }

        return new(raw, errors, finalSyncId);
    }

    private async Task<Result<List<YellerDto>>> GetDto(List<string> raw, string process = "Value retrieval")
    {
        if (raw.Count == 0)
            return Result.Failure<List<YellerDto>>("No IDs provided");

        List<YellerDto> master = [];
        int errors = 0;

        foreach (string id in raw)
        {
            if (errors >= errorLimit)
            {
                _logger.LogWarning(
                    "Reached error limit {ErrorLimit}. Retrieved: {Retrieved}. Process: {Process}",
                    errorLimit, master.Count, process);
                break;
            }

            await _throttle.WaitAsync();
            try
            {
                string uri = $"{_settings.YellerFinalEndpoint}/{id}";
                HttpResponseMessage response = await _client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    errors++;
                    _logger.LogWarning(
                        "Final response failed. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                        errors, errorLimit, master.Count, process);

                    if (response.ReasonPhrase is not null && response.ReasonPhrase.Contains("Unauthorized"))
                        break;

                    continue;
                }

                YellerDto? dto = await response.Content.ReadFromJsonAsync<YellerDto>();

                if (dto == null)
                {
                    errors++;
                    _logger.LogWarning(
                        "Null DTO. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                        errors, errorLimit, master.Count, process);
                    continue;
                }

                master.Add(dto);
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(
                    ex,
                    "Exception in DTO stage. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Exception Message: {Message}",
                    errors, errorLimit, master.Count, process, ex.Message);
            }
            finally { _throttle.Release(); }
        }

        if (master.Count == 0)
            return Result.Failure<List<YellerDto>>("Failed to retrieve DTOs");

        return Result.Success(master);
    }

    public async Task<Result<List<YellerDto>>> RefreshAsync()
    {
        return await GetAllAsync(refresh: true);
    }
}
