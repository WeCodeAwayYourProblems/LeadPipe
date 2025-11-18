using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface ICalliUpdateService
{
    Result<List<Plumbing>> GetData(FileInfo location);
    Task<Result> SaveDataAsync(List<Plumbing> plumbs);
}
