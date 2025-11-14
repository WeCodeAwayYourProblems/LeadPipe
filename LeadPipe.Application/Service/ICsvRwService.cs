using CSharpFunctionalExtensions;
using CsvHelper.Configuration;

namespace LeadPipe.Application.Service;

public interface ICsvRwService
{
    Result Append<TClass, TMap>(FileInfo path, IEnumerable<TClass> unparsed) where TMap : ClassMap<TClass>;
    Result<List<T>> Parse<T>(FileInfo path);
    Result Write<TClass, TMap>(FileInfo path, IEnumerable<TClass> unparsedObject) where TMap : ClassMap<TClass>;
    Result Write<TClass>(IEnumerable<TClass> unparsedObject, FileInfo path);
}
