using CsvHelper.Configuration;

namespace LeadPipe.Infrastructure.Test.FileManipulationTests.FileHelpers;

internal class TestFileMap : ClassMap<TestFile>
{
    internal TestFileMap()
    {
        string id = nameof(TestFile.Id);
        string name = nameof(TestFile.Name);
        string dateTime = nameof(DateTime);
        int index = 0;
        Map(m => m.Id).Index(index++).Name(id);
        Map(m => m.Name).Index(index++).Name(name);
        Map(m => m.DateTime).Index(index++).Name(dateTime);
    }
}