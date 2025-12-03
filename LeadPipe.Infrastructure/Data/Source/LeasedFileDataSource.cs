using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Service;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class LeasedFileDataSource(FileInfo file, ICsvRwService csv, IJsonRwService json, ILogger<LeasedFileDataSource> logging)
    : FileDataSource<LeasedDto, LeasedFileDataSource>(file, csv, json, logging), IDataSourceAsync<LeasedDto>
{ }
