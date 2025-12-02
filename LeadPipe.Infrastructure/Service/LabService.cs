using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Translate;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Service;

internal class LabService : ILabService
{
    #region Ctor
    private readonly HttpClient _client;
    private readonly ILogger<LabService> _logger;
    private readonly IDtoToVo _dtoToVo;
    private readonly ILabSettings _settings;
    private readonly IPlumbingRepository _plumbingRepo;
    private readonly SemaphoreSlim _throttle;

    public LabService(
        IHttpClientFactory httpClientFactory,
        ILabSettings settings,
        ILogger<LabService> logger,
        IDtoToVo dtoToVo,
        IPlumbingRepository plumbingRepo)
    {
        _client = httpClientFactory.CreateClient(settings.LabName!);
        _logger = logger;
        _dtoToVo = dtoToVo;
        _settings = settings;
        _plumbingRepo = plumbingRepo;
        _throttle = new(_settings.LabConcurrentMax!);
    }
    #endregion

    public async Task<Result<List<Plumbing>>> UpdateDataAsync(int errorLimit = 5)
    {
        // Retrieve persisted plumbing entities
        var existingResult = await _plumbingRepo.GetAllAsync();
        if (existingResult.IsFailure)
        {
            _logger.LogError("Failed to retrieve existing Plumbings: {Error}", existingResult.Error);
            return Result.Failure<List<Plumbing>>(existingResult.Error);
        }

        // Convert items for easy comparison later
        HashSet<(long PhoneNumber, DateTime Date)> existingPhoneDates = [.. existingResult.Value.Select(p => (p.PhoneNumber, p.Date))];

        if (existingResult.Value.Count == 0)
            return await GetLabsAsync(errorLimit);

        bool resume = true;
        var allDtos = new List<LabDto>();
        int errors = 0;
        int page = 1; // page is a page number, not an index

        while (resume)
        {
            Result<List<LabDto>> pageResult = await GetLabAsync(page);
            if (pageResult.IsFailure)
            {
                errors++;
                _logger.LogError("Failed to get page {Page}: {Error}", page, pageResult.Error);
                if (errors >= errorLimit)
                    return Result.Failure<List<Plumbing>>($"UpdateLabAsync failed after {errors} errors. Last error: {pageResult.Error}");
                page++;
                continue;
            }

            List<LabDto>? pageDtos = pageResult.Value;
            if (pageDtos is null || pageDtos.Count == 0)
                break;

            // Add records to list 
            // Stop if any record already exists
            // We can assume that further calls will also have records that exist
            foreach (var dto in pageDtos)
            {
                bool contains = existingPhoneDates.Contains((dto.PhoneNumber, dto.Date));
                if (!contains)
                    allDtos.Add(dto);
                else resume = false;
            }
            page++;
        }

        // Translate all DTOs to Plumbing
        var allPlumbings = allDtos.Select(_dtoToVo.Translate).ToList();
        return Result.Success(allPlumbings);
    }

    private static async void Wait(int sleepInterval = 500) => await Task.Delay(sleepInterval);
    internal async Task<Result<List<LabDto>>> GetLabAsync(int page = 1)
    {
        await _throttle.WaitAsync();
        List<LabDto>? dtos;
        try
        {
            var endpoint = _settings.LabPlumbing!;
            var response = await _client.GetAsync($"{endpoint}?per_page=15&page={page}");
            Wait();

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = $"Failed to get page {page}. Status code: {response.StatusCode}";
                _logger.LogError("Failed to get page {Page}. Status Code: {StatusCode}", page, response.StatusCode);
                return Result.Failure<List<LabDto>>(errorMessage);
            }

            var content = await response.Content.ReadAsStringAsync();
            dtos = JsonSerializer.Deserialize<List<LabDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching page {Page}: {Message}", page, ex.Message);
            return Result.Failure<List<LabDto>>($"Exception fetching page {page}: {ex.Message}");
        }
        finally
        {
            _throttle.Release();
        }
        return Result.Success(dtos ?? new List<LabDto>());
    }

    public async Task<Result<List<Plumbing>>> GetLabsAsync(int errorLimit = 5)
    {
        var allDtos = new List<LabDto>();
        int page = 1;
        int errors = 0;
        bool morePages = true;

        while (morePages)
        {
            var result = await GetLabAsync(page);
            if (result.IsFailure)
            {
                errors++;
                _logger.LogError("Failed to get page {Page}: {Error}", page, result.Error);
                if (errors >= errorLimit)
                    return Result.Failure<List<Plumbing>>($"GetLabsAsync failed after {errors} errors. Last error: {result.Error}");
                page++;
                continue;
            }

            var pageDtos = result.Value;
            if (pageDtos == null || pageDtos.Count == 0)
                morePages = false;
            else
            {
                allDtos.AddRange(pageDtos);
                page++;
            }
        }

        var allPlumbings = allDtos.Select(_dtoToVo.Translate).ToList();
        return Result.Success(allPlumbings);
    }
}
