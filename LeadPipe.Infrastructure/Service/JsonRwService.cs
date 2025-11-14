using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Service;

// All exceptions should be throw by the caller, because we don't have sufficient context in this class to understand WHY the error was thrown.
internal class JsonRwService : IJsonRwService
{
    public Result<List<T>> ReadFile<T>(FileInfo path)
    {
        try
        {
            string jsonStr = File.ReadAllText(path.FullName);
            return Deserialize<T>(jsonStr);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<T>>(ex.Message);
        }
    }
    public Result WriteToFile<T>(FileInfo path, List<T> items)
    {
        try
        {
            Result<string> result = Serialize(items);
            string strResult = result.IsSuccess
                ? result.Value
                : throw new Exception(result.Error);
            using StreamWriter writer = new(path.FullName);
            writer.Write(strResult);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    #region Private
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    internal/*fortestingonly*/ static Result<List<T>> Deserialize<T>(string jsonStr)
    {
        try
        {
            return JsonSerializer.Deserialize<List<T>>(jsonStr, _options)!;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<T>>($"The error occurred in the {nameof(Deserialize)} method.\nThis is the error: {ex.Message}");
        }
    }
    private static Result<string> Serialize<T>(IEnumerable<T> objects)
    {
        try
        {
            string result = JsonSerializer.Serialize(objects, _options);
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"The error occurred in the {nameof(Serialize)} method.\nThis is the error: {ex.Message}");
        }
    }
    #endregion
}
