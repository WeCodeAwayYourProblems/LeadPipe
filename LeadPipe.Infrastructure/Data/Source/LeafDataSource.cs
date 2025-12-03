using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Infrastructure.Data.Source;

public class LeafDataSource(ILeafService leaf, IVoToDto<Plumbing, LeafDto> voToDto) : IDataSourceAsync<LeafDto>
{
    private readonly ILeafService _leaf = leaf;
    private readonly IVoToDto<Plumbing, LeafDto> _voToDto = voToDto;
    public async Task<Result<List<LeafDto>>> LoadAsync()
    {
        Result<List<Plumbing>> get = await _leaf.GetAllAsync();
        if (get.IsFailure)
            return Result.Failure<List<LeafDto>>(get.Error);
        List<LeafDto> result = [.. get.Value.Select(_voToDto.Translate)];
        return result;
    }
}
