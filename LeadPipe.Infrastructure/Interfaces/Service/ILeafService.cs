using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Interfaces.Service;

public interface ILeafService
{
    Task<Result<List<Plumbing>>> GetAllAsync(int offset = 0, int errorLimit = 5);
    Task<Result<List<Plumbing>>> RefreshAsync(int errorLimit = 5);
}
