using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace LeadPipe.Infrastructure.Service;

internal class YellerClientService : IYellerService
{
    #region Ctor and Private Fields
    private readonly IHttpClientFactory _factory;
    private readonly IYellerSettings _settings;
    private readonly HttpClient _client;
    private readonly IDtoToVo<YellerDto, Plumbing> _dtoToVo;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _throttle;
    private const int errorLimit = 5;

    public YellerClientService(
        IHttpClientFactory factory,
        IYellerSettings settings,
        IDtoToVo<YellerDto, Plumbing> dtoToVo,
        ILogger<YellerClientService> logger)
    {
        _factory = factory;
        _settings = settings;
        _client = _factory.CreateClient(_settings.YellerName!);
        _dtoToVo = dtoToVo;
        _logger = logger;
        _throttle = new SemaphoreSlim(_settings.YellerConcurrentMax);
    }
    #endregion

    public async Task<Result<List<Plumbing>>> GetAllAsync()
    {
        const int limit = 20;
        var endpoint = $"{_settings.YellerPrelimEndpoint}?limit={limit}";
        var raw = new List<string>();

        int errors = 0;
        string process = "Id retrieval";

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

                    continue;
                }

                YellerHelperDto? value = await response.Content.ReadFromJsonAsync<YellerHelperDto>();

                if (value?.lead_ids == null)
                {
                    errors++;
                    _logger.LogWarning(
                        "Null or invalid prelim response. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                        errors, errorLimit, raw.Count, process);

                    continue;
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
                    "Exception in prelim stage. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                    errors, errorLimit, raw.Count, process);
            }
        }

        if (raw.Count == 0)
        {
            _logger.LogError(
                "Failed prelim retrieval. Errors: {Errors}. Process: {Process}",
                errors, process);

            return Result.Failure<List<Plumbing>>("Failed to retrieve data from API.");
        }

        Result<List<YellerDto>> dtoResult = await GetDto(raw);

        if (!dtoResult.IsSuccess)
            return Result.Failure<List<Plumbing>>(dtoResult.Error);

        List<Plumbing> final = [.. dtoResult.Value.Select(_dtoToVo.Translate)];
        return Result.Success(final);
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
                    "Exception in DTO stage. Errors: {Errors}/{Limit}. Retrieved: {Retrieved}. Process: {Process}",
                    errors, errorLimit, master.Count, process);
            }
            finally { _throttle.Release(); }
        }

        if (master.Count == 0)
            return Result.Failure<List<YellerDto>>("Failed to retrieve DTOs");

        return Result.Success(master);
    }

    public async Task<Result<List<Plumbing>>> RefreshAsync()
    {
        return await GetAllAsync();
    }
}
