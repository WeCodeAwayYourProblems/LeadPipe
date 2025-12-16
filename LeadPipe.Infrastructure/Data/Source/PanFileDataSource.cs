using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class PanFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<PanFileDataSource> logging)
    : FileDataSource<PanDto, PanFileDataSource>(new FileInfo(settings.PanSourceLoc!), csv, json, logging), IDataSourceAsync<PanDto>
{ }
