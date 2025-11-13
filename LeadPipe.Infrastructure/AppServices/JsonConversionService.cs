using CsvHelper.Configuration;
using LeadPipe.Infrastructure.CsvService;
using LeadPipe.Infrastructure.JsonService;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Services;

namespace LeadPipe.Infrastructure.AppServices;

internal class JsonConversionService : IJsonConversionService
{
    public Result<List<T>> Extract<T>(FileInfo jsonFile) => JsonRw.ReadFile<T>(jsonFile);

    public Result<FileInfo> SaveToCsv<T, TMap>(Result<List<T>> entities, FileInfo csvFile) where TMap : ClassMap<T>
    {
        try
        {
            if (entities.IsSuccess)
            {
                List<T> values = entities.Value;
                Result wrote = CsvRw.Write<T, TMap>(csvFile, values);
                Result<FileInfo> result = wrote.IsSuccess ? csvFile : Result.Failure<FileInfo>(wrote.Error);
                return result;
            }
            return Result.Failure<FileInfo>(entities.Error);
        }
        catch (Exception ex)
        {
            string error = $"The Json entities failed to save to Csv.\nException\n{ex.Message}";
            return Result.Failure<FileInfo>(error);
        }
    }
}