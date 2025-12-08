using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Service;

namespace LeadPipe.Infrastructure.Service;

internal class YellerClientService : IYellerGetterService
{
    public async Task<Result<List<Plumbing>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Result<List<Plumbing>>> RefreshAsync()
    {
        throw new NotImplementedException();
    }
}