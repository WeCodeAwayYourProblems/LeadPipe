using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces;

namespace LeadPipe.Infrastructure.Service;

internal class YellerClientService : IYellerService
{
    public Task<Result<List<Plumbing>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<Plumbing>>> RefreshAsync()
    {
        throw new NotImplementedException();
    }
}