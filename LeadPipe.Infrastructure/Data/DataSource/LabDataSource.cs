using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.DataSource;

public class LabDataSource(ILabService lab, IVoToDto<Plumbing, LabDto> voToDto) : IDataSourceAsync<LabDto>
{
    private readonly ILabService _lab = lab;
    private readonly IVoToDto<Plumbing, LabDto> _voToDto = voToDto;
    public async Task<Result<List<LabDto>>> LoadAsync(bool _ = false)
    {
        Result<List<Plumbing>> get = await _lab.GetLabsAsync();
        if (get.IsFailure)
            return Result.Failure<List<LabDto>>(get.Error);
        List<LabDto> result = [.. get.Value.Select(_voToDto.Translate)];
        return result;
    }

    public async Task<Result<List<LabDto>>> RefreshAsync(bool _ = false)
    {
        Result<List<Plumbing>> get = await _lab.UpdateDataAsync();
        if (get.IsFailure)
            return Result.Failure<List<LabDto>>(get.Error);
        List<LabDto> result = [.. get.Value.Select(_voToDto.Translate)];
        return result;
    }
}
