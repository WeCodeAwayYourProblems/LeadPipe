using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;

namespace LeadPipe.Application.Manager;

public interface IFileRWManager
{
    Result<List<T>> ManageRead<T>(FileInfo file);
    Result ManageWrite<T>(FileInfo file, List<T> data);
}

public sealed class FileRWManager(IFileRWService service) : IFileRWManager
{
    private readonly IFileRWService _service = service;
    private const string errMsg = "File extension not supported: ";
    public Result<List<T>> ManageRead<T>(FileInfo file)
    {
        return file.Extension switch
        {
            ".csv" => _service.ExtractCsv<T>(file),
            ".json" => _service.ExtractJson<T>(file),
            _ => Result.Failure<List<T>>(errMsg + file.Extension)
        };
    }
    public Result ManageWrite<T>(FileInfo file, List<T> data)
    {
        return file.Extension switch
        {
            ".csv" => _service.SaveToCsv<T>(data, file),
            ".json" => _service.SaveToJson<T>(data, file),
            _ => Result.Failure(errMsg + file.Extension)
        };
    }
}
