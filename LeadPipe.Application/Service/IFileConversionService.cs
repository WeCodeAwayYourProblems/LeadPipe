using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IFileConversionService
{
    Result<List<T>> ExtractCsv<T>(FileInfo csv);
    Result<List<T>> ExtractJson<T>(FileInfo jsonFile);
    Result<FileInfo> SaveToJson<T>(Result<List<T>> input, FileInfo jsonFile);
    Result<FileInfo> SaveToCsv<T>(Result<List<T>> entities, FileInfo csvFile);
}
