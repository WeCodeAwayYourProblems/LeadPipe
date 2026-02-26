using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using System.Net.Http.Json;

namespace LeadPipe.Infrastructure.Service;

internal class CatManClientService(ICatManSettings settings, IHttpClientFactory factory) : ICatManService
{
    private readonly ICatManSettings _settings = settings;
    private readonly HttpClient _client = factory.CreateClient(settings.CatManClientName!);

    private const int MaxRequestsPerSecond = 8;
    private static readonly TimeSpan RateLimitDelay = TimeSpan.FromMilliseconds(1000d / MaxRequestsPerSecond);

    private Uri BuildCallUri(DateTime start, DateTime end)
    {
        var formattedStart = start.ToString(_settings.CatManDateFormat!);
        var formattedEnd = end.ToString(_settings.CatManDateFormat!);

        var uri = $"accounts/{_settings.CatAccountId}/calls.json" +
                  $"?start_date = {formattedStart}&end_date={formattedEnd}";

        return new Uri(uri);
    }

    private async Task<Result<CatmanDto>> GetCallAsync(Uri uri)
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return Result.Failure<CatmanDto>(response.ReasonPhrase ?? $"{nameof(CatmanDto)} request failed");

            var value = await response.Content.ReadFromJsonAsync<CatmanDto>();

            if (value is not null)
                return Result.Success(value);
            var raw = await response.Content.ReadAsStringAsync();

            return Result.Failure<CatmanDto>(
                string.IsNullOrWhiteSpace(raw)
                ? $"{nameof(CatmanDto)} deserialization returned null"
                : raw);
        }
        catch (Exception ex) { return Result.Failure<CatmanDto>(ex.ToString()); }
    }

    public async Task<Result<List<CatmanDto>>> GetAllAsync(DateTime start, DateTime end)
    {
        List<CatmanDto> results = [];

        Uri? nextUri = BuildCallUri(start, end);
        string? afterValueOfPreviousPage = null;

        while (nextUri is not null)
        {
            var result = await GetCallAsync(nextUri);

            if (result.IsFailure)
                return Result.Failure<List<CatmanDto>>(result.Error);

            results.Add(result.Value);

            await Task.Delay(RateLimitDelay);

            if (string.IsNullOrWhiteSpace(result.Value.NextPage) || result.Value.After == afterValueOfPreviousPage)
                break;

            afterValueOfPreviousPage = result.Value.After;
            nextUri = new(result.Value.NextPage);
        }
        return Result.Success(results);
    }
}
