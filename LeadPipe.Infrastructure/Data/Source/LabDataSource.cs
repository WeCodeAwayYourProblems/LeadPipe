using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Source;

public class LabDataSource(ILabService lab, IVoToDto<Plumbing, LabDto> voToDto) : IDataSourceAsync<LabDto>
{
    private readonly ILabService _lab = lab;
    private readonly IVoToDto<Plumbing, LabDto> _voToDto = voToDto;
    public async Task<Result<List<LabDto>>> LoadAsync()
    {
        Result<List<Plumbing>> get = await _lab.GetLabsAsync();
        if (get.IsFailure)
            return Result.Failure<List<LabDto>>(get.Error);
        List<LabDto> result = [.. get.Value.Select(_voToDto.Translate)];
        return result;
    }

    public async Task<Result<List<LabDto>>> RefreshAsync()
    {
        Result<List<Plumbing>> get = await _lab.UpdateDataAsync();
        if (get.IsFailure)
            return Result.Failure<List<LabDto>>(get.Error);
        List<LabDto> result = [.. get.Value.Select(_voToDto.Translate)];
        return result;
    }
}
