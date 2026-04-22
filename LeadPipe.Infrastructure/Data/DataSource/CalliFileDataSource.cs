using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.DataSource;

public sealed class CalliFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<CalliFileDataSource> logging)
    : FileDataSource<CalliDto, CalliFileDataSource>(new FileInfo(settings.CalliLoc!.Source!), csv, json, logging), IDataSourceAsync<CalliDto>
{
    protected override Result<List<CalliDto>> FlattenInvalid(Result<List<CalliDto>> fileContents)
    {
        if (fileContents.IsFailure) return fileContents;

        return fileContents.Value.Where(
        v => 
            !string.IsNullOrWhiteSpace(v.Phone) && 
            v.Phone.Length >= 10 && 
            long.TryParse(v.Phone, out var phone) && 
            phone > 0
        ).ToList();
    }
}
