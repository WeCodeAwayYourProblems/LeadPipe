using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Interfaces.Service;

namespace LeadPipe.Infrastructure.Service;

internal class FileConversionService(ICsvRwService csv, IJsonRwService json) : IFileRWService
{
    private readonly ICsvRwService _csv = csv;
    private readonly IJsonRwService _json = json;
    public Result<List<T>> ExtractCsv<T>(FileInfo csv) => _csv.ReadFile<T>(csv);
    public Result<List<T>> ExtractJson<T>(FileInfo jsonFile) => _json.ReadFile<T>(jsonFile);

    public Result<FileInfo> SaveToCsv<T>(Result<List<T>> entities, FileInfo csvFile)
    {
        try
        {
            if (entities.IsSuccess)
            {
                List<T> values = entities.Value;
                Result wrote = _csv.Write(values, csvFile);
                Result<FileInfo> result = wrote.IsSuccess ? csvFile : Result.Failure<FileInfo>(wrote.Error);
                return result;
            }
            return Result.Failure<FileInfo>(entities.Error);
        }
        catch (Exception ex)
        {
            return Result.Failure<FileInfo>(ex.Message);
        }
    }
    public Result<FileInfo> SaveToJson<T>(Result<List<T>> input, FileInfo jsonFile)
    {
        try
        {
            if (input.IsSuccess)
            {
                List<T> values = input.Value;
                Result wrote = _json.WriteToFile<T>(jsonFile, values);
                Result<FileInfo> result = wrote.IsSuccess ? jsonFile : Result.Failure<FileInfo>(wrote.Error);
                return result;
            }
            return Result.Failure<FileInfo>(input.Error);
        }
        catch (Exception ex)
        {
            return Result.Failure<FileInfo>(ex.Message);
        }
    }
}