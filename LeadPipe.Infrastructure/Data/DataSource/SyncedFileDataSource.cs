using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Service;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.DataSource;

public abstract class SyncedFileDataSource<TDto>(
    FileInfo file,
    ICsvRwService csv,
    IJsonRwService json,
    ILogger<SyncedFileDataSource<TDto>> logger,
    ISyncStateRepository state,
    IClock clock
    ) : IDataSourceAsync<TDto>
{
    #region Readonly
    
    private readonly FileInfo _file = file;
    private readonly ICsvRwService _csv = csv;
    private readonly IJsonRwService _json = json;
    protected readonly ILogger<SyncedFileDataSource<TDto>> _logger = logger;
    private readonly ISyncStateRepository _state = state;
    protected readonly IClock _clock = clock;
    
    #endregion

    #region Abstract/Virtual

    protected abstract SyncKey SyncKey { get; }
    protected abstract Source Source { get; }
    protected abstract DateTimeOffset GetRowTimestamp(TDto row);
    protected abstract Result<List<TDto>> FlattenInvalid(List<TDto> fileContents);
    protected virtual DateTimeOffset DefaultLatestDate => _clock.UtcNow.AddDays(-30);
    protected virtual async Task<Result> FileExistenceCheck(FileInfo file)
    {
        if (!file.Exists)
        {
            string newContent = string.Empty;
            _logger.LogWarning("{ProvidedFile} does not exist. Creating provided file with the following content: \"{NewContent}\".", file.FullName, newContent);
            await File.WriteAllTextAsync(file.FullName, newContent);
        }

        return Result.Success(); // Failed results return an error, and we don't want to do that here
    }

    #endregion

    #region Private/Protected
    
    private async Task<Result<List<TDto>>> LoadFileAsync()
    {
        Result exists = await FileExistenceCheck(_file);
        if (exists.IsFailure)
            return Result.Failure<List<TDto>>(exists.Error);

        Result<List<TDto>> result = _file.Extension switch
        {
            ".csv" => _csv.ReadFile<TDto>(_file),
            ".json" => _json.ReadFile<TDto>(_file),
            _ => Result.Failure<List<TDto>>($"Unknown file type: \"{_file.Extension}\"")
        };
        return result;
    }

    protected async Task SyncLatestState(List<TDto> contents)
    {
        if (contents.Count == 0)
        {
            _logger.LogWarning("Contents of {ProvidedFile} returned an empty list and could not be synced", _file.FullName);
            return;
        }

        DateTimeOffset latest = contents.Max(GetRowTimestamp);

        var state = new SyncStateEntity
        {
            BusinessId = BusinessId.BuildBusinessId(Source, SyncKey),
            LastSyncUtc = latest.UtcDateTime,
            UnixLastSyncUtc = latest.ToUnixTimeSeconds()
        };

        await _state.UpsertRangeAsync([state]);
    }

    protected async Task<DateTimeOffset> GetDateOfLatestSync()
    {
        Result<SyncStateEntity> latest = await _state.GetByKeyAsync(Source, SyncKey);
        if (latest.IsFailure)
            return DefaultLatestDate;

        DateTimeOffset result = DateTimeOffset.FromUnixTimeSeconds(latest.Value.UnixLastSyncUtc); // Use UnixLastSyncUtc to prevent date drift

        return result;
    }

    #endregion

    #region Public

    public async Task<Result<List<TDto>>> LoadAsync(bool _ = default)
    {
        Result<List<TDto>> contents = await LoadFileAsync();

        if (contents.IsFailure)
        {
            _logger.LogError("Error while reading {ProvidedFile}. Error {Error}", _file.FullName, contents.Error);
            return contents;
        }

        Result<List<TDto>> flattened = FlattenInvalid(contents.Value);
        if (flattened.IsFailure)
            return flattened;

        await SyncLatestState(flattened.Value);

        return flattened;
    }

    public async Task<Result<List<TDto>>> RefreshAsync(bool withDetails)
    {
        // Load latest sync date
        DateTimeOffset latestSyncDate = await GetDateOfLatestSync();

        // Load file contents
        Result<List<TDto>> contents = await LoadFileAsync();
        if (contents.IsFailure)
            return contents;

        // Filter file contents to latest
        List<TDto> latestContent = [.. contents.Value.Where(v => GetRowTimestamp(v) >= latestSyncDate)];

        Result<List<TDto>> flattened = FlattenInvalid(latestContent);
        if (flattened.IsFailure)
            return flattened;

        await SyncLatestState(flattened.Value);

        return flattened;
    }

    #endregion
}