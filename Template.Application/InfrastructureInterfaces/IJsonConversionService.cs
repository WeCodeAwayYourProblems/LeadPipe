using CsvHelper.Configuration;

namespace Template.Application.InfrastructureInterfaces;

public interface IJsonConversionService
{
    List<T> Extract<T>(FileInfo jsonFile);
    Dictionary<bool, FileInfo> SaveToCsv<T, TMap>(List<T> entities, FileInfo csvFile) where TMap : ClassMap<T>;
}
