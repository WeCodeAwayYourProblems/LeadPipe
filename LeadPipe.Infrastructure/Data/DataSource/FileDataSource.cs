using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Data.DataSource;

public abstract class FileDataSource<TDto, TSource>(FileInfo file, ICsvRwService csv, IJsonRwService json, ILogger<TSource> logging) : IDataSourceAsync<TDto>
{
    private readonly FileInfo _file = file;
    private readonly ICsvRwService _csv = csv;
    private readonly IJsonRwService _json = json;
    private readonly ILogger<TSource> _logger = logging;
    public async Task<Result<List<TDto>>> LoadAsync(bool _ = false)
    {
        if (!_file.Exists)
        {
            _logger.LogWarning("Provided file does not exist {ProvidedFile}", _file.FullName);
            await File.WriteAllTextAsync(_file.FullName, string.Empty);
        }

        var result = _file.Extension switch
        {
            ".csv" => _csv.ReadFile<TDto>(_file),
            ".json" => _json.ReadFile<TDto>(_file),
            _ => Result.Failure<List<TDto>>("Unknown file type")
        };
        Result<List<TDto>> flattened = FlattenInvalid(result);
        return flattened;
    }

    public async Task<Result<List<TDto>>> RefreshAsync(bool _ = false) => await LoadAsync();

    protected abstract Result<List<TDto>> FlattenInvalid(Result<List<TDto>> fileContents);

}
