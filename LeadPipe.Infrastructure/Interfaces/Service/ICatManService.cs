using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Interfaces.Service;

public interface ICatManService
{
    Task<Result<List<CatManDto>>> GetAllAsync(DateTime start, DateTime end);
}