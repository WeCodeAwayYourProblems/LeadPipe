using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

public class CalliFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<CalliFileDataSource> logging)
    : FileDataSource<CalliDto, CalliFileDataSource>(new FileInfo(settings.CalliSourceLoc!), csv, json, logging), IDataSourceAsync<CalliDto>
{ }
