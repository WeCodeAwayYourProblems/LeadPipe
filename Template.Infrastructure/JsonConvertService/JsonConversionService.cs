using Template.Application.InfrastructureInterfaces;
using CsvHelper.Configuration;
using Template.Infrastructure.CsvService;
using Infrastructure.JsonService;

namespace Template.Infrastructure.JsonConvertService;

internal class JsonConversionService : IJsonConversionService
{
    public List<T> Extract<T>(FileInfo jsonFile)
    {
        return JsonRw.DeserializeFile<T>(jsonFile.FullName);
    }

    public Dictionary<bool, FileInfo> SaveToCsv<T, TMap>(List<T> entities, FileInfo csvFile) where TMap : ClassMap<T>
    {
        bool success = false;
        try
        {
            CsvRw.WriteToCsv<T, TMap>(csvFile.FullName, entities);
            success = true;
        }
        catch (Exception ex)
        {
            // TODO: Log the exception
        }
        return new() { { success, csvFile } };
    }

}