using CSharpFunctionalExtensions;
using System.Net.Http.Json;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Translate;
using System.Text;
using LeadPipe.Domain.FunctionalObjects;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service;

internal class LeafClientService(ILeafSettings settings, IDtoToVo dtoTranslate, IJsonRwService json) : ILeafClientService
{
    #region Private
    private readonly ILeafSettings _settings = settings;
    private readonly IDtoToVo _dto = dtoTranslate;
    private readonly IJsonRwService _json = json;
    private Uri LeafThreadUrl(int offset = 0, int limit = 1000) => new($"{_settings.LeafThreadsEndpoint}?limit={limit}&offset={offset}");
    private Uri LeafMessagesUrl(string thread, int limit = 100, string type = "sms") => new($"{_settings.LeafThreadsEndpoint}/{thread}{_settings.LeafMessagesEndpoint}?limit={limit}&type={type}&offset=0");
    private static async Task<Result<T>> GetSingleAsync<T>(Uri url, HttpClient client)
    {
        // Attempt to make the call
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            Wait();
            if (response.IsSuccessStatusCode)
            {
                T? value = await response.Content.ReadFromJsonAsync<T>();
                if (value is not null)
                {
                    return value!;
                }

                string str = await response.Content.ReadAsStringAsync();
                string error = string.IsNullOrWhiteSpace(str) || str.Length == 0
                    ? "Parsing failure. The process of reading the results from Json failed. The results somehow became null."
                    : str;

                return Result.Failure<T>(error);
            }
            return Result.Failure<T>(response.ReasonPhrase);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex.Message);
        }
    }
    private static async void Wait(int sleepInterval = 500) => await Task.Delay(sleepInterval);
    #endregion

    #region Public
    public HttpClient GetClient(IHttpClientFactory factory)
    {
        HttpClient client = factory.CreateClient(_settings.LeafName!);
        return client;
    }
    public async Task<Result<List<Plumbing>>> GetAsync(HttpClient client, int offset = 0, int errorLimit = 5, int limit = 1000)
    {
        int errorCount = 0;
        List<Plumbing> master = [];
        List<LeafDto> raw = [];
        StringBuilder builder = new();

        bool resume = true;
        bool failure = false;
        while (resume)
        {
            if (errorCount == errorLimit)
            {
                failure = true;
                break;
            }

            try
            {
                // Call the api
                Uri newurl = LeafThreadUrl(offset, limit);
                Result<List<LeafDto>> result = await GetSingleAsync<List<LeafDto>>(newurl, client);

                // Unwrap result
                if (result.IsSuccess)
                {
                    List<LeafDto> value = result.Value;
                    value.ForEach(raw.Add);

                    // Translate from dto to vo
                    List<Plumbing> translation = [.. value.Select(_dto.Translate)];
                    translation.ForEach(master.Add);
                    resume = value.Count == limit;
                }
                else
                {
                    errorCount++;
                    builder.Append(result.Error);
                }
                offset += limit;
            }
            catch { errorCount++; }
        }

        if (master.Count == 0)
            return Result.Failure<List<Plumbing>>($"Something went wrong and values were not retrieved.\nErrors, if any:\n{builder}");

        // Save raw, assuming there were raw values
        if (raw.Count > 0)
        {
            FileInfo file = new(FolderFinder.GetLocalFile(nameof(Infrastructure), ".info", "RawLeaf.json"));
            Result saved = _json.WriteToFile(file, raw);
            if (saved.IsFailure) {/*In theory, we log failures here*/}
        }

        if (failure)
            return Result.Failure<List<Plumbing>>($"Reached error limit. Error limit: {errorLimit} attempts.\nErrors:\n{builder}");

        return master;
    }
    public Result<List<Message>>[] GetMessages(HttpClient client, List<LeafDto> leafs)
    {
        List<Task<Result<List<Message>>>> tasks = new(leafs.Count);
        foreach (LeafDto leaf in leafs)
        {
            // Retrieve the thread id
            if (leaf.uuid is null)
                continue;
            string threadid = leaf.uuid;
            Uri uri = LeafMessagesUrl(threadid);

            // Retrieve new list
            Task<Result<List<Message>>> messagesResultTask = GetSingleAsync<List<Message>>(uri, client);
            tasks.Add(messagesResultTask);

            Thread.Sleep(1000 / (5 - 2));
        }

        Task<Result<List<Message>>[]> completedTask = Task.WhenAll(tasks);
        Result<List<Message>>[] result = ConvertTasks(completedTask);

        return result;
    }

    #endregion

    #region Internal
    internal static Result<List<Message>>[] ConvertTasks(Task<Result<List<Message>>[]> completedTask)
    {
        if (completedTask.IsCompletedSuccessfully)
        {
            Result<List<Message>>[] taskResult = completedTask.Result;
            IEnumerable<Result<List<Message>>> result = taskResult.Select(r =>
            {
                // Unwrapt the value
                List<Message> value;
                if (r.IsSuccess) value = r.Value;
                else return Result.Failure<List<Message>>(r.Error);

                // Convert values
                List<Message> messages = [.. value.Select(v => v)];

                return Result.Success(messages);
            });
            return [.. result];
        }
        else
        {
            string e = completedTask.Exception is not null
                ? completedTask.Exception.Message
                : "Unknown exception";
            return [Result.Failure<List<Message>>(e)];
        }
    }
    #endregion


}
