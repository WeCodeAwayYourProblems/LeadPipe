using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.DataSource;

internal sealed class LabFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<LabFileDataSource> logging)
    : FileDataSource<LabDto, LabFileDataSource>(new FileInfo(settings.LabLoc!.Source!), csv, json, logging), IDataSourceAsync<LabDto>
{
    protected override Result<List<LabDto>> FlattenInvalid(Result<List<LabDto>> fileContents)
    {
        throw new NotImplementedException();
    }
}
internal sealed class SyncedLabFileDataSource(
    IInfrastructureSettings settings,
    ICsvRwService csv,
    IJsonRwService json,
    ILogger<SyncedLabFileDataSource> logger,
    ISyncStateRepository sync,
    IClock clock) : SyncedFileDataSource<LabDto>(new FileInfo(settings.LabLoc!.Source!), csv, json, logger, sync, clock)
{
    protected override SyncKey SyncKey => SyncKey.Plumbing;
    protected override Source Source => Source.Lab;

    protected override DateTimeOffset GetRowTimestamp(LabDto row) => DateTimeOffset.TryParse(row.created_at?.date_utc, out var dateTime) ? dateTime : DefaultLatestDate;
    
    protected override Result<List<LabDto>> FlattenInvalid(List<LabDto> fileContents)
    {
        throw new NotImplementedException();
    }

}