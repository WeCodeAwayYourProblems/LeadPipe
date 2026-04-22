using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Service;

internal class LabService : ILabService
{
    #region Ctor

    private readonly HttpClient _client;
    private readonly ILogger<LabService> _logger;
    private readonly ILabSettings _settings;
    private readonly SemaphoreSlim _throttle;
    private readonly JsonSerializerOptions _options;
    private const int _limit = 15; // 15 is the magic number for this api
    public LabService(
        IHttpClientFactory httpClientFactory,
        ILabSettings settings,
        ILogger<LabService> logger)
    {
        _client = httpClientFactory.CreateClient(settings.LabName!);
        _logger = logger;
        _settings = settings;
        _throttle = new(_settings.LabConcurrentMax!);
        _options = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #endregion

    public async Task<Result<List<LabDto>>> UpdateDataAsync(int errorLimit = 5, CancellationToken ct = default)
    {
        List<LabDto> allDtos = [];
        int totalErrors = 0;
        var pageErrors = 0;
        int page = 1; // page is a page number, not an index, which is why it's 1-based and not zero-based

        while (true)
        {
            Result<LabHelperDto> pageResult = await GetLabAsync(page, ct);
            if (pageResult.IsFailure)
            {
                totalErrors++;
                pageErrors++;
                _logger.LogError("Failed to get page {Page}: {Error}. Errors on page: {PageErrors}. Total error count: {TotalErrors}",
                    page,
                    pageResult.Error,
                    pageErrors,
                    totalErrors);
                if (totalErrors >= errorLimit)
                    return Result.Failure<List<LabDto>>($"{nameof(UpdateDataAsync)} failed after {totalErrors} errors. Last error: {pageResult.Error}");
                continue;
            }

            pageErrors = 0;
            LabHelperDto? pageDto = pageResult.Value;
            if (pageDto is null || pageDto.data?.items is null)
                break;

            IEnumerable<LabDto> labs = pageDto.data.items
                .Where(i => i?.labDto is not null)
                .Select(i => i!.labDto!);
            allDtos.AddRange(labs);

            if (pageDto.data?.next_page is null)
                break;

            page++;
        }

        return Result.Success(allDtos);
    }

    internal async Task<Result<LabHelperDto>> GetLabAsync(int page = 1, CancellationToken ct = default)
    {
        await _throttle.WaitAsync(ct);
        LabHelperDto? dtos;
        try
        {
            var endpoint = _settings.LabPlumbing!;
            var response = await _client.GetAsync($"{endpoint}?per_page={_limit}&page={page}", ct);
            await Task.Delay(250, ct);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = $"Failed to get page {page}. Status code: {response.StatusCode}";
                _logger.LogError("{Service}: Failed to get page {Page}. Status Code: {StatusCode}", nameof(LabService), page, response.StatusCode);
                return Result.Failure<LabHelperDto>(errorMessage);
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            dtos = JsonSerializer.Deserialize<LabHelperDto>(content, _options);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Service}: Exception fetching page {Page}: {Message}", nameof(LabService), page, ex.Message);
            return Result.Failure<LabHelperDto>($"Exception fetching page {page}: {ex}");
        }
        finally
        {
            _throttle.Release();
        }
        return Result.Success(dtos ?? new());
    }

    public async Task<Result<List<LabDto>>> GetLabsAsync(int errorLimit = 5, CancellationToken ct = default)
    {
        List<LabDto> allDtos = [];
        int page = 1;
        int totalErrors = 0;
        int pageErrors = 0;
        bool morePages = true;

        while (morePages)
        {
            Result<LabHelperDto> result = await GetLabAsync(page, ct);
            if (result.IsFailure)
            {
                totalErrors++;
                pageErrors++;
                _logger.LogError("Failed to get page {Page}: {Error}. Errors on page: {PageErrors}. Total error count: {TotalErrors}",
                    page,
                    result.Error,
                    pageErrors,
                    totalErrors);
                if (totalErrors >= errorLimit || result.Error.Contains("unauthorized", StringComparison.InvariantCultureIgnoreCase))
                    return Result.Failure<List<LabDto>>($"{nameof(GetLabsAsync)} failed after {totalErrors} errors. Last error: {result.Error}");
                
                if (pageErrors >= errorLimit / 2)
                    page++;
                continue;
            }

            pageErrors = 0;
            List<LabDto> pageDtos = result.Value.data?.items is not null
                ? [.. result.Value.data.items.Where(i => i!.labDto is not null).Select(i => i!.labDto!)]
                : [];
            if (pageDtos == null || pageDtos.Count == 0)
                morePages = false;
            else
            {
                allDtos.AddRange(pageDtos);
                page++;
            }
        }

        return Result.Success(allDtos);
    }

}
