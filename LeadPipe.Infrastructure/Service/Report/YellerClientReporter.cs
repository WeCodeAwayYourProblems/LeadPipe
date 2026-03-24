using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Yeller)]
public sealed class YellerClientReporter : IReport<ReportYeller>
{
    #region Ctor and Fields
    private const int _throttleCount = 90;
    private const int _waitTimeLimit = 10;
    private readonly RateLimiter _rateLimiter = new(90, TimeSpan.FromSeconds(1));

    private readonly IYellerSettings _settings;
    private readonly HttpClient _client;
    private readonly SemaphoreSlim _throttle;
    private readonly string _endpoint;
    private readonly ILogger<YellerClientReporter> _logger;

    public YellerClientReporter(
        IHttpClientFactory factory,
        IYellerSettings settings,
        ILogger<YellerClientReporter> logger
    )
    {
        _settings = settings;
        _client = factory.CreateClient(_settings.YellerReporterName!);
        _throttle = new(_throttleCount);
        _endpoint = settings.YellerReportEndpoint!;
        _logger = logger;
    }
    #endregion

    public async Task<Result> ReportData(List<ReportYeller> data)
    {
        if (data.Count == 0)
            return Result.Failure("Input data is empty");

        List<List<ReportYeller>> batches = [.. ChunkBy(data, 200)];
        int uploadedBatchNumber = 0;
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        List<Task<Result>> tasks = [.. batches.Select(async batch =>
        {
            if (!await _throttle.WaitAsync(TimeSpan.FromSeconds(_waitTimeLimit)))
            {
                return Result.Failure($"Throttle wait timed out after {_waitTimeLimit}");
            }

            try
            {
                await _rateLimiter.WaitForAvailabilityAsync();

                string json = JsonSerializer.Serialize(new { events = batch }, jsonOptions);

                using StringContent content = new(json, Encoding.UTF8, "application/json");
                
                AsyncPolicy<HttpResponseMessage> policy = GetRetryPolicy();
                Context context = new() { ["BatchSize"] = batch.Count };
                HttpResponseMessage response = await policy.ExecuteAsync(
                    ctx => _client.PostAsync(_endpoint, content),
                    context
                );

                if (response.IsSuccessStatusCode)
                {
                    int currentBatchNumber = Interlocked.Increment(ref uploadedBatchNumber);
                    _logger.LogInformation("Uploaded batch of {Count} events successfully. On batch {UploadedBatchNumber}/{TotalBatches}",
                        batch.Count,
                        currentBatchNumber,
                        batches.Count
                    );

                    return Result.Success();
                }

                string body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("HTTP {StatusCode}: {Body}", response.StatusCode, body);
                return Result.Failure($"HTTP {(int)response.StatusCode}: {body}");
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.ToString());
            }
            finally
            {
                _throttle.Release();
            }
        })];

        var results = await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Finished uploading {TotalBatches} batches with {TotalEvents} events",
            batches.Count,
            data.Count
        );

        return Result.Combine(" | ", results);
    }

    private static IEnumerable<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
    {
        for (int i = 0; i < source.Count; i += chunkSize)
            yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
    }

    private AsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var policy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests || (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    int batchSize = context.ContainsKey("BatchSize") ? (int)context["BatchSize"] : 0;
                    _logger.LogWarning(
                        "Retry {RetryAttempt} after {Delay}s due to {Reason} for batch size {BatchSize}",
                        retryAttempt,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString(),
                        batchSize
                    );
                }
            );
        return policy;
    }

}

// Keep these together for now
public sealed class YellerJsonReporter(
    IYellerSettings settings,
    IJsonRwService json
    ) : IReport<ReportYeller>
{
    private readonly IYellerSettings _settings = settings;
    private readonly IJsonRwService _json = json;

    public async Task<Result> ReportData(List<ReportYeller> d)
    {
        FileInfo loc = new(_settings.YellerJsonReporterLoc!);
        var result = await _json.WriteToFileAsync(loc, d);
        return result;
    }
}
