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

public class LeafClientService : ILeafService
{
    #region Ctor and Private Fields
    public LeafClientService(
        ILeafSettings settings,
        IDtoToVo<LeafDto, Plumbing> dtoTranslate,
        IJsonRwService json,
        IHttpClientFactory factory,
        IPlumbingRepository repo,
        IFileService file,
        ILogger<LeafClientService> logger)
    {
        _logger = logger;
        _settings = settings;
        _dto = dtoTranslate;
        _json = json;
        _factory = factory;
        _repo = repo;
        _file = file;
        _client = _factory.CreateClient(_settings.LeafName!);
        _throttle = new(_settings.LeafConcurrentMax);
    }

    private readonly ILogger<LeafClientService> _logger;
    private readonly ILeafSettings _settings;
    private readonly IDtoToVo<LeafDto, Plumbing> _dto;
    private readonly IJsonRwService _json;
    private readonly IHttpClientFactory _factory;
    private readonly IPlumbingRepository _repo;
    private readonly IFileService _file;
    private readonly HttpClient _client;
    private readonly SemaphoreSlim _throttle;
    private Uri LeafThreadUrl(int offset = 0, int limit = 1000) =>
        new($"{_settings.LeafThreadsEndpoint}?limit={limit}&offset={offset}");
    private Uri LeafMessagesUrl(string thread, int limit = 10, string type = "sms") =>
        new($"{_settings.LeafThreadsEndpoint}/{thread}/{_settings.LeafMessagesEndpoint}?limit={limit}&type={type}&offset=0");
    private static async void Wait(int sleepInterval = 500) => await Task.Delay(sleepInterval);
    private static async Task<Result<T>> GetSingleAsync<T>(Uri url, HttpClient client)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            Wait();
            if (response.IsSuccessStatusCode)
            {
                T? value = await response.Content.ReadFromJsonAsync<T>();
                return value is not null
                    ? Result.Success(value)
                    : Result.Failure<T>("Parsing failure: JSON returned null.");
            }
            return Result.Failure<T>(response.ReasonPhrase);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex.Message);
        }
    }
    #endregion

    #region Public
    /// <summary>
    /// Refreshes threads from the API starting after the last Leaf-sourced item in the database.
    /// Falls back to full refresh if none exist.
    /// </summary>
    public async Task<Result<List<Plumbing>>> RefreshAsync(int errorLimit = 5)
    {
        Result<List<PlumbingEntity>> plumbingEntities = await _repo.GetAllAsync(source: Source.Leaf);

        if (plumbingEntities.IsFailure)
        {
            _logger.LogWarning("Repository call failed: {Error}. Performing full refresh.", plumbingEntities.Error);
            return await GetAllAsync(errorLimit: errorLimit);
        }

        List<PlumbingEntity> leafPlumbing = [.. plumbingEntities.Value.Where(v => v.Source == Source.Leaf)];
        if (leafPlumbing.Count == 0)
        {
            _logger.LogWarning("Database returned no items for {Source}. Performing full refresh.", Source.Leaf);
            return await GetAllAsync(errorLimit: errorLimit);
        }

        int offset = leafPlumbing.Count - 1; // Api uses index-style offsets
        return await GetAllAsync(offset, errorLimit);
    }

    public async Task<Result<List<Plumbing>>> GetAllAsync(int offset = 0, int errorLimit = 5)
    {
        const int limit = 1000;
        int errorCount = 0;
        List<Plumbing> master = [];
        List<LeafDto> raw = [];

        bool resume = true;
        bool failure = false;
        while (resume)
        {
            if (errorCount == errorLimit)
            {
                failure = true;
                break;
            }

            await _throttle.WaitAsync();
            try
            {
                // Call the api
                Uri newurl = LeafThreadUrl(offset, limit);
                Result<List<LeafDto>> result = await GetSingleAsync<List<LeafDto>>(newurl, _client);

                // Unwrap result
                if (result.IsSuccess)
                {
                    // Add dtos to raw list
                    List<LeafDto> value = result.Value;
                    raw.AddRange(value);

                    resume = value.Count == limit;
                    offset += limit;
                }
                else
                {
                    errorCount++;
                    _logger.LogWarning("API call failed at offset {Offset}. Error count: {ErrorCount}. Error Limit: {ErrorLimit}. Error: {Error}", offset, errorCount, errorLimit, result.Error);
                }
            }
            catch (Exception e)
            {
                errorCount++;
                _logger.LogError(e, "Exception occurred while calling API. Offset {Offset}. Error Count: {ErrorCount}. Error Limit: {ErrorLimit}", offset, errorCount, errorLimit);
            }
            finally { _throttle.Release(); }
        }

        if (raw.Count > 0)
        {
            // Save raw, assuming there were raw values
            FileInfo file = new(_file.GetLocalFile(nameof(Infrastructure), ".info", "RawLeaf.json"));
            Result saved = _json.WriteToFile(file, raw);
            if (saved.IsFailure)
                _logger.LogError("Failed to write raw dtos to file {FileName} due to {Error}", file.FullName, saved.Error);

            // Refresh messages
            Result<List<Message>>[] msgs = await GetMessagesAsync(raw);
            List<LeafDto> updatedLeafs = Update(raw, msgs);

            // Translate from dto to vo
            List<Plumbing> translation = [.. updatedLeafs.Select(_dto.Translate)];
            translation.ForEach(master.Add);
        }

        if (master.Count == 0)
        {
            string msg = "Failed to retrieve any data from the api";
            _logger.LogError("{Message}", msg);
            return Result.Failure<List<Plumbing>>(msg);
        }

        if (failure)
            _logger.LogError("Reached error limit {ErrorLimit}", errorLimit);

        return Result.Success(master);
    }
    #endregion

    #region Internal
    internal List<LeafDto> Update(List<LeafDto> raw, Result<List<Message>>[] msgs)
    {
        Dictionary<string, Message[]> messageLookup = msgs
            .Select(r =>
            {
                // Log Failures
                if (r.IsFailure)
                    _logger.LogError("Failed to retrieve messages. {Error}", r.Error);
                return r;
            })
            .Where(r => r.IsSuccess && r.Value is not null)
            .SelectMany(r => r.Value)
            .Where(m => m.thread is not null)
            .GroupBy(m => m.thread!)
            .ToDictionary(
                g => g.Key,
                g => g.ToArray() // Message[]
            );

        // Reassign messages
        foreach (LeafDto leaf in raw)
        {
            if (leaf.uuid is null)
            {
                leaf.messages = [];
                continue;
            }

            leaf.messages =
                messageLookup.TryGetValue(leaf.uuid, out var messages)
                ? messages
                : [];
        }
        return raw;
    }
    internal async Task<Result<List<Message>>[]> GetMessagesAsync(List<LeafDto> leafs)
    {
        List<Task<Result<List<Message>>>> tasks = leafs
            .Where(l => l.uuid is not null)
            .Select(async leaf =>
            {
                await _throttle.WaitAsync();
                try
                {
                    Uri uri = LeafMessagesUrl(leaf.uuid!);
                    return await GetSingleAsync<List<Message>>(uri, _client);
                }
                finally
                {
                    _throttle.Release();
                }
            }).ToList();
        return await Task.WhenAll(tasks);
    }
    #endregion
}
