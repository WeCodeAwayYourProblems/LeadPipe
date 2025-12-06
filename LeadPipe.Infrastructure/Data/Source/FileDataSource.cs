using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

public abstract class FileDataSource<TDto, TSource>(FileInfo file, ICsvRwService csv, IJsonRwService json, ILogger<TSource> logging) : IDataSourceAsync<TDto>
{
    private readonly FileInfo _file = file;
    private readonly ICsvRwService _csv = csv;
    private readonly IJsonRwService _json = json;
    private readonly ILogger<TSource> _logger = logging;
    public async Task<Result<List<TDto>>> LoadAsync()
    {
        if (!_file.Exists)
        {
            _logger.LogWarning("Provided file does not exist {ProvidedFile}", _file.FullName);
            await File.WriteAllTextAsync(_file.FullName, string.Empty);
        }

        return _file.Extension switch
        {
            ".csv" => _csv.ReadFile<TDto>(_file),
            ".json" => _json.ReadFile<TDto>(_file),
            _ => Result.Failure<List<TDto>>("Unknown file type")
        };
    }

    public async Task<Result<List<TDto>>> RefreshAsync()
    {
        return await LoadAsync();
    }
}
