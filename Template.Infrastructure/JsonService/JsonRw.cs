using System.Text.Json;

namespace Template.Infrastructure.JsonService;

// TODO: Use Result here instead of bare objects
internal static class JsonRw
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    internal static List<T> DeserializeFile<T>(string path)
    {
        string jsonStr = File.ReadAllText(path);
        return Deserialize<T>(jsonStr);
    }
    internal static List<T> Deserialize<T>(string jsonStr)
    {
        return JsonSerializer.Deserialize<List<T>>(jsonStr, _options)!;
    }
    internal static string Serialize<T>(IEnumerable<T> objects)
    {
        return JsonSerializer.Serialize(objects, _options);
    }
    internal static void SerializeToFile<T>(string path, List<T> items)
    {
        string result = Serialize(items);
        using StreamWriter writer = new(path);
        writer.Write(result);
    }
}
