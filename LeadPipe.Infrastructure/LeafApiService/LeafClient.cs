using CSharpFunctionalExtensions;
using System.Net.Http.Json;
using LeadPipe.Application.InfrastructureInterfaces;
using LeadPipe.Domain.Dto;

namespace LeadPipe.Infrastructure.LeafApiService;

internal class LeafClient(ILeafSettings settings) : ILeafClient
{
    #region Public
    public HttpClient GetClient(IHttpClientFactory factory)
    {
        HttpClient client = factory.CreateClient(settings.LeafName!);
        return client;
    }
    public async Task<Result<List<LeafDto>>> GetAsync(HttpClient client, int offset = 0, int errorLimit = 5, int limit = 1000)
    {
        int errorCount = 0;
        List<LeafDto> master = [];

        bool resume = true;
        while (resume)
        {
            if (errorCount == errorLimit)
                return Result.Failure<List<LeafDto>>($"Reached error limit. Error limit: {errorLimit} attempts.");

            try
            {
                // Call the api
                Uri newurl = LeafThreadUrl(offset, limit);
                Result<List<LeafDto>> result = await GetSingleAsync<List<LeafDto>>(newurl, client);

                // Unwrap result
                if (result.IsSuccess)
                {
                    List<LeafDto> value = result.Value;
                    value.ForEach(master.Add);
                    resume = value.Count == limit;
                }
                else
                {
                    errorCount++;
                    return result;
                }
                offset += limit;
            }
            catch { errorCount++; }
        }
        if (master.Count == 0)
            return Result.Failure<List<LeafDto>>("Something went wrong and values were not retrieved.");
        return master;
    }
    public Task<Result<List<Message>>[]> GetMessages(HttpClient client, List<LeafDto> leafs)
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
        return completedTask;
    }
    #endregion

    #region Internal
    internal Uri LeafThreadUrl(int offset = 0, int limit = 1000) => new($"{settings.LeafBase}{settings.LeafThreadsEndpoint}?limit={limit}&offset={offset}");
    internal Uri LeafMessagesUrl(string thread, int limit = 100, string type = "sms") => new($"{settings.LeafBase}{settings.LeafThreadsEndpoint}/{thread}{settings.LeafMessagesEndpoint}?limit={limit}&type={type}&offset=0");
    internal static async Task<Result<T>> GetSingleAsync<T>(Uri url, HttpClient client)
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
    #endregion

    #region Private
    private static async void Wait(int sleepInterval = 500) => await Task.Delay(sleepInterval);
    #endregion
}
