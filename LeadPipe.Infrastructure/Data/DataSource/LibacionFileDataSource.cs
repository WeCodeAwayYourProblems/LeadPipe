using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.DataSource;

internal class LibacionFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<LibacionFileDataSource> logging)
    : FileDataSource<LibacionDto, LibacionFileDataSource>(new FileInfo(settings.LibacionLoc!.Source!), csv, json, logging), IDataSourceAsync<LibacionDto>
{
    protected override Result<List<LibacionDto>> FlattenInvalid(Result<List<LibacionDto>> fileContents)
    {
        if (fileContents.IsFailure) return fileContents;
        return fileContents.Value.Where(v => PhoneNumber.TryParse(v.PhoneNumber, out var _)).ToList();
    }
}
