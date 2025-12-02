using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager
{
    public interface ILabManager
    {
        Task<Result<List<Plumbing>>> Manage(int errorLimit = 5, bool update = true);
    }
}