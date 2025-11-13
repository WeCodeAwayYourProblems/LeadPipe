using CSharpFunctionalExtensions;
using System.Net.Http.Json;
using LeadPipe.Application.Services;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Application.DataInterfaces.Dto;
using LeadPipe.Infrastructure.SettingsInterfaces;

namespace LeadPipe.Infrastructure.Services;

internal class LeafClientService(ILeafSettings settings) : ILeafClientService
{
    #region Public
    public HttpClient GetClient(IHttpClientFactory factory)
    {
        HttpClient client = factory.CreateClient(settings.LeafName!);
        return client;
    }
    public async Task<Result<List<ILeafDto>>> GetAsync(HttpClient client, int offset = 0, int errorLimit = 5, int limit = 1000)
    {
        int errorCount = 0;
        List<ILeafDto> master = [];

        bool resume = true;
        while (resume)
        {
            if (errorCount == errorLimit)
                return Result.Failure<List<ILeafDto>>($"Reached error limit. Error limit: {errorLimit} attempts.");

            try
            {
                // Call the api
                Uri newurl = LeafThreadUrl(offset, limit);
                Result<List<LeafDto>> result = await GetSingleAsync<List<LeafDto>>(newurl, client);

                // Unwrap result
                if (result.IsSuccess)
                {
                    List<ILeafDto> value = (List<ILeafDto>)result.Value.Select(v => v as ILeafDto);
                    value.ForEach(master.Add);
                    resume = value.Count == limit;
                }
                else
                {
                    errorCount++;
                    return Result.Failure<List<ILeafDto>>(result.Error);
                }
                offset += limit;
            }
            catch { errorCount++; }
        }
        if (master.Count == 0)
            return Result.Failure<List<ILeafDto>>("Something went wrong and values were not retrieved.");
        return master;
    }
    public Result<List<IMessage>>[] GetMessages(HttpClient client, List<ILeafDto> leafs)
    {
        List<Task<Result<List<Message>>>> tasks = new(leafs.Count);
        foreach (ILeafDto leaf in leafs)
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
        Result<List<IMessage>>[] result = ConvertTasks(completedTask);

        return result;
    }

    #endregion

    #region Internal
    internal static Result<List<IMessage>>[] ConvertTasks(Task<Result<List<Message>>[]> completedTask)
    {
        if (completedTask.IsCompletedSuccessfully)
        {
            Result<List<Message>>[] taskResult = completedTask.Result;
            IEnumerable<Result<List<IMessage>>> result = taskResult.Select(r =>
            {
                // Unwrapt the value
                List<Message> value;
                if (r.IsSuccess) value = r.Value;
                else return Result.Failure<List<IMessage>>(r.Error);

                // Convert values
                List<IMessage> messages = [.. value.Select(v => v)];

                return Result.Success(messages);
            });
            return [.. result];
        }
        else
        {
            string e = completedTask.Exception is not null
                ? completedTask.Exception.Message
                : "Unknown exception";
            return [Result.Failure<List<IMessage>>(e)];
        }
    }
    #endregion

    #region Private
    private Uri LeafThreadUrl(int offset = 0, int limit = 1000) => new($"{settings.LeafThreadsEndpoint}?limit={limit}&offset={offset}");
    private Uri LeafMessagesUrl(string thread, int limit = 100, string type = "sms") => new($"{settings.LeafThreadsEndpoint}/{thread}{settings.LeafMessagesEndpoint}?limit={limit}&type={type}&offset=0");
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
}
