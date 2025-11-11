using CSharpFunctionalExtensions;
using LeadPipe.Domain.Dto;
namespace LeadPipe.Application.InfrastructureInterfaces;

public interface ILeafClient
{
    Task<Result<List<LeafDto>>> GetAsync(HttpClient client, int offset = 0, int errorLimit = 5, int limit = 1000);
    HttpClient GetClient(IHttpClientFactory factory);
    Task<Result<List<Message>>[]> GetMessages(HttpClient client, List<LeafDto> leafs);
}