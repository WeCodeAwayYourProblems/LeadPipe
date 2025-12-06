using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class LibacionFileDataSource(FileInfo file, ICsvRwService csv, IJsonRwService json, ILogger<LibacionFileDataSource> logging)
    : FileDataSource<LibacionDto, LibacionFileDataSource>(file, csv, json, logging), IDataSourceAsync<LibacionDto>
{ }
