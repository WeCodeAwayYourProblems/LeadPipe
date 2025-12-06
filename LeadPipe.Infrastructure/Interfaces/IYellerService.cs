using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Interfaces;

public interface IYellerService
{
    Task<Result<List<Plumbing>>> GetAllAsync();
    Task<Result<List<Plumbing>>> RefreshAsync();
}
