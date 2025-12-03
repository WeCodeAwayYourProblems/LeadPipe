using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Service;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

public class CalliFileDataSource(FileInfo file, ICsvRwService csv, IJsonRwService json, ILogger<CalliFileDataSource> logging)
    : FileDataSource<CalliDto, CalliFileDataSource>(file, csv, json, logging), IDataSourceAsync<CalliDto>
{ }
