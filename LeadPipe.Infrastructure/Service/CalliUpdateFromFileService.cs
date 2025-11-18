using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Infrastructure.Service;

internal class CalliUpdateFromFileService(
    ICsvRwService csv,
    IDtoToVo dtovo,
    IVoToEntity toEntity,
    IPlumbingRepository plumb) : ICalliUpdateService
{
    private readonly ICsvRwService _csv = csv;
    private readonly IDtoToVo _dtovo = dtovo;
    private readonly IVoToEntity _toEntity = toEntity;
    private readonly IPlumbingRepository _pr = plumb;
    public Result<List<Plumbing>> GetData(FileInfo location)
    {
        Result<List<CalliCsvDto>> raw = _csv.Parse<CalliCsvDto>(location);
        if (raw.IsFailure)
            return Result.Failure<List<Plumbing>>($"Failed to parse file:\n{location.FullName}");
        List<CalliCsvDto> value = raw.Value;
        List<Plumbing> data = [.. value.Select(_dtovo.Translate)];
        return data;
    }
    public async Task<Result> SaveDataAsync(List<Plumbing> plumbs)
    {
        List<PlumbingEntity> conversion = [.. plumbs.Select(_toEntity.Translate)];
        Result result = await _pr.AddRangeAsync(conversion);
        return result;
    }
}
