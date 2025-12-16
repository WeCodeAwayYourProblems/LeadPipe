using CSharpFunctionalExtensions;
using CsvHelper.Configuration;

namespace LeadPipe.Infrastructure.Interfaces.Service;

public interface ICsvRwService
{
    Result Append<TClass, TMap>(FileInfo path, IEnumerable<TClass> unparsed) where TMap : ClassMap<TClass>;
    Result<List<T>> ReadFile<T>(FileInfo path);
    Result Write<TClass, TMap>(FileInfo path, IEnumerable<TClass> unparsedObject) where TMap : ClassMap<TClass>;
    Result Write<TClass>(IEnumerable<TClass> unparsedObject, FileInfo path);
    Task<Result> WriteAsync<TClass>(IEnumerable<TClass> input,  FileInfo path, CancellationToken ct);
}
