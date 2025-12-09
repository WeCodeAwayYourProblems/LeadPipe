using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service;

internal class YellerClientService : IYellerService
{
    private readonly IHttpClientFactory _factory;
    private readonly IYellerSettings _settings;
    private readonly HttpClient? _client;

    public YellerClientService(
        IHttpClientFactory factory, IYellerSettings settings
    )
    {
        _factory = factory;
        _settings = settings;
        _client = _factory.CreateClient(_settings.YellerName!);
    }
    public Task<Result<List<Plumbing>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Result<List<Plumbing>>> RefreshAsync()
    {
        throw new NotImplementedException();
    }

}
