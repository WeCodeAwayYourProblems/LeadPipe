using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Template.Infrastructure.CsvService;

internal static class CsvRw
{
    private static readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;
    private const string _delimiter = ",";
    private static readonly CsvConfiguration _config = new(_cultureInfo)
    {
        Delimiter = _delimiter,
        HasHeaderRecord = true,
        NewLine = Environment.NewLine
    };
    private static readonly CsvConfiguration _noHeader = new(_cultureInfo)
    {
        Delimiter = _delimiter,
        HasHeaderRecord = false,
        NewLine = Environment.NewLine
    };
    internal static List<T> ParseFromCsv<T>(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, _config);
        List<T> records = csv.GetRecords<T>().ToList();
        return records;
    }
    public static void WriteToCsv<TClass, TMap>(string path, IEnumerable<TClass> unparsedObject) where TMap : ClassMap
    {
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, _config);
        csv.Context.RegisterClassMap<TMap>();
        csv.WriteRecords(unparsedObject);
    }
    public static void AppendToCsv<TClass, TMap>(string path, IEnumerable<TClass> unparsed) where TMap : ClassMap
    {
        using var stream = File.Open(path, FileMode.Append);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, _noHeader);
        csv.WriteRecords(unparsed);
    }
}
