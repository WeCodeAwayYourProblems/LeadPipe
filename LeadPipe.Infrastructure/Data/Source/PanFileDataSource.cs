using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class PanFileDataSource(FileInfo file, ICsvRwService csv, IJsonRwService json, ILogger<PanFileDataSource> logging)
    : FileDataSource<PanDto, PanFileDataSource>(file, csv, json, logging), IDataSourceAsync<PanDto>
{ }
