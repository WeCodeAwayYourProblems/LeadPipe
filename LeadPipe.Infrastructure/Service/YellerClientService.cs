using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Service;

internal class YellerClientService(
    IHttpClientFactory factory,
    IYellerSettings settings,
    ILogger<YellerClientService> logger,
    ISyncStateRepository sync
    ) : IYellerService
{
    #region Ctor and Private Fields

    private readonly IYellerSettings _settings = settings;
    private readonly HttpClient _client = factory.CreateClient(settings.YellerGetterName!);
    private readonly ILogger _logger = logger;
    private readonly ISyncStateRepository _sync = sync;
    private readonly SemaphoreSlim _throttle = new(settings.YellerConcurrentMax);
    private const int errorLimit = 5;
    const int prelimPageSize = 20;
    const int maxEvents = 100;
    record YellerFetchResult(List<string> Raw, int Errors, string FinalId);

    #endregion

    #region Implementation
    public async Task<Result<List<YellerDto>>> RefreshAsync(CancellationToken ct = default) => await GetAllAsync(refresh: true, ct);

    public async Task<Result<List<YellerDto>>> GetAllAsync(bool refresh, CancellationToken ct = default)
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
            string endpoint = $"{_settings.YellerPrelimEndpoint1}{yellerId}{_settings.YellerPrelimEndpoint2}?limit={prelimPageSize}";

            // Retrieve sync state id
            // If the syncStateId is empty, then we will retrieve all data
            Result<SyncStateEntity> syncState =
                Result.Failure<SyncStateEntity>("Still Confirming how codes work on this api");
            //await _sync.GetByIdAsync(BusinessId.From(yellerId));
            string syncStateId = syncState.IsSuccess && refresh
                ? syncState.Value.BusinessId.ToString()
                : string.Empty;

            YellerFetchResult data = await GetData(yellerId, process, endpoint, syncStateId, ct);

            // Aggregate raw and errors
            allRaw.AddRange(data.Raw);
            allErrors += data.Errors;

            if (!string.IsNullOrWhiteSpace(data.FinalId))
                finalIds[yellerId] = data.FinalId;
        }

        // Distinct raw
        allRaw = [.. allRaw.Distinct()];
        await SyncState(finalIds);

        // Check that raw is not empty
        if (allRaw.Count == 0)
        {
            _logger.LogError(
                "Failed prelim retrieval. Errors: {Errors}. Process: {Process}",
                allErrors, process);

            return Result.Failure<List<YellerDto>>("Failed to retrieve data from API.");
        }

        // Hydrate dtos with data by fetching data in all ids
        Result<List<YellerDto>> dtoResult = await GetDto(allRaw, ct: ct);
        if (dtoResult.IsFailure)
            return dtoResult;

        // Retrieve events
        List<YellerDto> notNullIds = [.. dtoResult.Value.Where(r => r.id is not null)];
        Result<List<YellerDto>> dtoWithEvents = await GetEvents(notNullIds, ct);

        return dtoWithEvents;
    }

    #endregion

    #region Events
    private async Task<Result<List<YellerDto>>> GetEvents(List<YellerDto> allRaw, CancellationToken ct = default)
    {
        List<YellerDto> deepCopy = new(allRaw.Count);
        foreach (YellerDto dto in allRaw)
        {
            if (dto.id is null)
            {
                // Keep for debugging
                var dcp = DeepCopySetter(dto, []);
                deepCopy.Add(dcp);
                continue;
            }

            HashSet<string> seenEventIds = [];
            HashSet<string> seenCursors = [];
            List<Event> collectedEvents = [];
            string? cursor = null;
            while (true)
            {
                string endpoint = cursor is null
                    ? GetEventEndpoint(dto.id!) // these are private methods now, not local methods
                    : GetOlderEndpoint(dto.id!, cursor);

                Result<YellerEventDto> result = await GetEvent(endpoint, ct);

                if (result.IsFailure || result.Value.events is null)
                    break;

                var page = result.Value;

                if (!TryAddEvents(page.events))
                    break;

                Event? oldest = RetrieveOldestEvent(page);
                if (oldest?.cursor is null)
                    break;

                string nextCursor = oldest.cursor;
                if (!seenCursors.Add(nextCursor))
                    break;
                cursor = nextCursor;
            }

            // Keep for debugging
            var dc = DeepCopySetter(dto, collectedEvents);
            deepCopy.Add(dc);

            #region Local: Must be inside loop
            bool TryAddEvents(IEnumerable<Event> events)
            {
                foreach (var e in events)
                    if (e.id is null || seenEventIds.Add(e.id))
                    {
                        collectedEvents.Add(e);
                        if (collectedEvents.Count >= maxEvents)
                            return false;
                    }
                return true;
            }
            #endregion

        }

        return deepCopy;

    }
    string GetEventEndpoint(string id) => $"{_settings.YellerFinalEndpoint!}/{id}{_settings.YellerEventsEndpoint!}";
    string GetOlderEndpoint(string id, string specialId) => GetEventEndpoint(id) + $"?{_settings.YellerRecursion!}={specialId}";

    private static YellerDto DeepCopySetter(YellerDto dto, List<Event> events)
    {
        YellerDto result = dto.Clone();
        result.events = new YellerEventDto() { events = [.. events] };
        return result;
    }

    private static Event? RetrieveOldestEvent(YellerEventDto value)
    {
        if (value.events is null || value.events.Length == 0)
            return null;

        Event? oldestWithTime = null;

        foreach (var e in value.events)
        {
            if (e.time_created is null)
                continue;

            if (oldestWithTime is null || e.time_created < oldestWithTime.time_created)
                oldestWithTime = e;
        }

        return oldestWithTime ?? value.events[^1];
    }

    private async Task<Result<YellerEventDto>> GetEvent(string endpoint, CancellationToken ct)
    {
        int errors = 0;
        const string process = "Event retrieval";
        string errorMsg = string.Empty;
        while (true)
        {
            if (errors >= errorLimit)
            {
                _logger.LogWarning(
                    "Reached error limit {ErrorLimit}. Process: {Process}. Endpoint: {Endpoint}",
                    errorLimit, process, endpoint);
                errorMsg = $"Reached error limit ({errorLimit} errors) in process {process}";
                break;
            }

            await _throttle.WaitAsync(ct);
            try
            {
                await Task.Delay(100 * errors * errors, ct);
                HttpResponseMessage response = await _client.GetAsync(endpoint, ct);

                if (!response.IsSuccessStatusCode)
                {
                    errors++;
                    _logger.LogError(
                        "Response failure ({Reason}). Errors: {Errors}/{Limit}. Process: {Process}. Endpoint: {Endpoint}",
                        response.ReasonPhrase, errors, errorLimit, process, endpoint);

                    // If 404 NotFound, then there's a problem with the endpoint
                    if (response.ReasonPhrase is not null && (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound))
                    {
                        errorMsg = response.ReasonPhrase; //response.StatusCode == HttpStatusCode.NotFound ? response.ReasonPhrase  + " Note: Fix Endpoint" : response.ReasonPhrase;
                        break;
                    }

                    continue;
                }

                YellerEventDto? value = await response.Content.ReadFromJsonAsync<YellerEventDto>(cancellationToken: ct);

                if (value is null)
                {
                    errors++;
                    _logger.LogWarning(
                        "Null or invalid response. Errors: {Errors}/{Limit}. Process: {Process}. Endpoint: {Endpoint}",
                        errors, errorLimit, process, endpoint);

                    continue;
                }

                return Result.Success(value!);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(
                    ex,
                    "Exception. Errors: {Errors}/{Limit}. Process: {Process}. Exception Message: {Message}",
                    errors, errorLimit, process, ex.Message);
            }
            finally { _throttle.Release(); }
        }
        return Result.Failure<YellerEventDto>(errorMsg);
    }

    #endregion

    #region Helpers
    private async Task SyncState(Dictionary<string, string> finalIds)
    {
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
    }

    private async Task<YellerFetchResult> GetData(string yellerId, string process, string endpoint, string syncStateId, CancellationToken ct = default)
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
                await _throttle.WaitAsync(ct);
                HttpResponseMessage response = await _client.GetAsync(endpoint, ct);

                if (!response.IsSuccessStatusCode)
                {
                    errors++;
                    _logger.LogError(
                        "Response failure ({Reason}). Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Endpoint: {Endpoint}",
                        response.ReasonPhrase, errors, errorLimit, raw.Count, process, endpoint);

                    if (response.ReasonPhrase is not null && response.StatusCode == HttpStatusCode.Unauthorized)
                        break;

                    continue;
                }

                string stringVal = await response.Content.ReadAsStringAsync(ct);
                YellerHelperDto? value = JsonSerializer.Deserialize<YellerHelperDto>(stringVal);

                if (value?.lead_ids == null || value.lead_ids.Length == 0)
                {
                    errors++;
                    _logger.LogWarning(
                        "Null or invalid prelim response. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Endpoint: {EndPoint}",
                        errors, errorLimit, raw.Count, process, endpoint);

                    continue;
                }

                // Ids are retrieved in a chronological stack
                // Therefore, the sync state id must be the most recent chronologically,
                // which is the first id of the first call
                if (callCount == 0)
                    finalSyncId = value.lead_ids[0];
                callCount++;

                string nextId = value.lead_ids[^1];
                endpoint = $"{_settings.YellerPrelimEndpoint1}{yellerId}{_settings.YellerPrelimEndpoint2}?limit={prelimPageSize}&{_settings.YellerPrelimId}={nextId}";

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
                    "Exception in prelim stage. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Endpoint: {Endpoint}. Exception Message: {Message}.",
                    errors, errorLimit, raw.Count, process, endpoint, ex.Message);
            }
            finally { _throttle.Release(); }
        }

        return new(raw, errors, finalSyncId);
    }

    private async Task<Result<List<YellerDto>>> GetDto(List<string> raw, string process = "Value retrieval", CancellationToken ct = default)
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

            await _throttle.WaitAsync(ct);
            string endpoint = $"{_settings.YellerFinalEndpoint}/{id}";
            try
            {
                HttpResponseMessage response = await _client.GetAsync(endpoint, ct);

                if (!response.IsSuccessStatusCode)
                {
                    errors++;
                    _logger.LogWarning(
                        "Final response failed. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Endpoint: {Endpoint}",
                        errors, errorLimit, master.Count, process, endpoint);

                    if (response.ReasonPhrase is not null && response.StatusCode == HttpStatusCode.Unauthorized)
                        break;

                    continue;
                }

                var stringVal = await response.Content.ReadAsStringAsync(ct);
                YellerDto? dto = JsonSerializer.Deserialize<YellerDto>(stringVal);

                if (dto == null)
                {
                    errors++;
                    _logger.LogWarning(
                        "Null DTO. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Endpoint: {Endpoint}",
                        errors, errorLimit, master.Count, process, endpoint);
                    continue;
                }

                master.Add(dto);
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(
                    ex,
                    "Exception in DTO stage. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}. Endpoint: {Endpoint}. Exception Message: {Message}",
                    errors, errorLimit, master.Count, process, endpoint, ex.Message);
            }
            finally { _throttle.Release(); }
        }

        if (master.Count == 0)
            return Result.Failure<List<YellerDto>>("Failed to retrieve DTOs");

        return Result.Success(master);
    }

    #endregion

}
