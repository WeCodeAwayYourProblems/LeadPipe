using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Interfaces.Service;

public interface IYellerGetterService
{
    Task<Result<List<Plumbing>>> GetAllAsync();
    Task<Result<List<Plumbing>>> RefreshAsync();
}
