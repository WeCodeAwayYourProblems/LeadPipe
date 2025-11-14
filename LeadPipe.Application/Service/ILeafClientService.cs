using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
namespace LeadPipe.Application.Service;

public interface ILeafClientService
{
    Task<Result<List<Plumbing>>> GetAsync(HttpClient client, int offset = 0, int errorLimit = 5, int limit = 1000);
    HttpClient GetClient(IHttpClientFactory factory);
}