using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.DataSource;

public sealed class LatherFileDataSource(
    IInfrastructureSettings settings,
    ICsvRwService csv,
    IJsonRwService json,
    ILogger<LatherFileDataSource> logging)
    : FileDataSource<LatherDto, LatherFileDataSource>(new FileInfo(settings.LatherLoc!.Source!), csv, json, logging), IDataSourceAsync<LatherDto>
{
    protected override Result<List<LatherDto>> FlattenInvalid(Result<List<LatherDto>> fileContents)
    {
        if (fileContents.IsFailure) return fileContents;
        return fileContents.Value.Where(v => PhoneNumber.TryParse(v.Phone, out PhoneNumber _)).ToList();
    }
}