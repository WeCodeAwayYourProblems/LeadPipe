using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.DataSource;

internal class LeasedFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<LeasedFileDataSource> logging)
    : FileDataSource<LeasedDto, LeasedFileDataSource>(new FileInfo(settings.LeasedLoc!.Source!), csv, json, logging), IDataSourceAsync<LeasedDto>
{
    protected override Result<List<LeasedDto>> FlattenInvalid(Result<List<LeasedDto>> fileContents)
    {
        if (fileContents.IsFailure) return fileContents;
        return fileContents.Value.Where(v => PhoneNumber.TryParse(v.PhoneNumber, out var _)).ToList();
    }
}
