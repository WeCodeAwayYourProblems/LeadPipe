using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Interfaces.Service;

public interface ILabService
{
    Task<Result<List<Plumbing>>> GetLabsAsync(int errorLimit = 5);
    Task<Result<List<Plumbing>>> UpdateDataAsync(int errorLimit = 5);
}
