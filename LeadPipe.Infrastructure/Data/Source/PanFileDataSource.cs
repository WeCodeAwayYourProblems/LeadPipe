using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class PanFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<PanFileDataSource> logging)
    : FileDataSource<PanDto, PanFileDataSource>(new FileInfo(settings.PanSourceLoc!), csv, json, logging), IDataSourceAsync<PanDto>
{
    protected override Result<List<PanDto>> FlattenInvalid(Result<List<PanDto>> fileContents)
    {
        if (fileContents.IsFailure) return fileContents;
        return fileContents.Value.Where(v => PhoneNumber.TryParse(v.Number, out var _)).ToList();
    }
}
