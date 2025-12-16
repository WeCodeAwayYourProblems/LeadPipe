using CSharpFunctionalExtensions;
using CsvHelper;
using CsvHelper.Configuration;
using LeadPipe.Infrastructure.Interfaces.Service;
using System.Globalization;

namespace LeadPipe.Infrastructure.Service;

internal class CsvRwService : ICsvRwService
{
    #region Private
    private static readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;
    private const string _delimiter = ",";
    private readonly CsvConfiguration _config = new(_cultureInfo)
    {
        Delimiter = _delimiter,
        HasHeaderRecord = true,
        NewLine = Environment.NewLine
    };
    private readonly CsvConfiguration _noHeader = new(_cultureInfo)
    {
        Delimiter = _delimiter,
        HasHeaderRecord = false,
        NewLine = Environment.NewLine
    };
    private static string CsvException(string path, Exception ex, string action)
        => $"Failed to perform the following action on the csv file: {action}\nFile path: {path}\nException message: {ex.Message}";
    #endregion

    #region Public
    public Result<List<T>> ReadFile<T>(FileInfo path)
    {
        try
        {
            using StreamReader reader = new(path.FullName);
            using CsvReader csv = new(reader, _config);
            List<T> records = [.. csv.GetRecords<T>()];
            return records;
        }
        catch (Exception ex)
        { return Result.Failure<List<T>>(CsvException(path.FullName, ex, nameof(ReadFile))); }
    }
    public Result Write<TClass, TMap>(FileInfo path, IEnumerable<TClass> unparsedObject) where TMap : ClassMap<TClass>
    {
        try
        {
            using StreamWriter writer = new(path.FullName);
            using CsvWriter csv = new(writer, _config);
            csv.Context.RegisterClassMap<TMap>();
            csv.WriteRecords(unparsedObject);
            return Result.Success();
        }
        catch (Exception ex)
        {
            var exception = CsvException(path.FullName, ex, nameof(Write));
            return Result.Failure(exception);
        }
    }
    public Result Write<TClass>(IEnumerable<TClass> unparsedObject, FileInfo path)
    {
        try
        {
            using StreamWriter writer = new(path.FullName);
            using CsvWriter csv = new(writer, _config);
            csv.WriteRecords(unparsedObject);
            return Result.Success();
        }
        catch (Exception ex)
        {
            var exception = CsvException(path.FullName, ex, nameof(Write));
            return Result.Failure(exception);
        }
    }
    public Result Append<TClass, TMap>(FileInfo path, IEnumerable<TClass> unparsed) where TMap : ClassMap<TClass>
    {
        try
        {
            using FileStream stream = File.Open(path.FullName, FileMode.Append);
            using StreamWriter writer = new(stream);
            using CsvWriter csv = new(writer, _noHeader);
            csv.WriteRecords(unparsed);
            return Result.Success();
        }
        catch (Exception ex)
        { return Result.Failure(CsvException(path.FullName, ex, nameof(Append))); }
    }

    public async Task<Result> WriteAsync<TClass>(
    IEnumerable<TClass> unparsedObject,
    FileInfo path,
    CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = new FileStream(
                path.FullName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, _config);

            // CsvHelper is synchronous here
            csv.WriteRecords(unparsedObject);

            // Ensure buffered data is flushed asynchronously
            await writer.FlushAsync();
            await stream.FlushAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var exception = CsvException(path.FullName, ex, nameof(WriteAsync));
            return Result.Failure(exception);
        }
    }

    #endregion
}
