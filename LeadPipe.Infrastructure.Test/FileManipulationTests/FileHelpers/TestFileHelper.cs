using LeadPipe.Domain.FunctionalObjects;

namespace LeadPipe.Infrastructure.Test.FileManipulationTests.FileHelpers;

internal class TestFileHelper
{
    #region Private
    private string JsonFileLocation => FolderFinder.GetLocalFile(nameof(Test), "TestFileLoc", "FileManipulationTests/TestFile.json");
    private string CsvFileLocation => FolderFinder.GetLocalFile(nameof(Test), "TestFileLoc", "FileManipulationTests/TestFile.csv");
    #endregion

    #region Internal
    internal static TestFile ParseStringToTestFile(string id, string name, string dateTime, out int intDefault, out DateTime dtDefault, out int idResult, out DateTime dtResult)
    {
        // Parse the non-string properties into target primitives
        intDefault = 0;
        dtDefault = DateTime.MinValue;
        var idInt = int.TryParse(id, out idResult)
            ? idResult
            : intDefault;
        var dateTimeParsed = DateTime.TryParse(dateTime, out dtResult)
            ? dtResult
            : dtDefault;
        return new() { Id = idInt, Name = name, DateTime = dateTimeParsed };
    }

    internal static string AccessTestFile(TestFileType type)
    {
        var file = new TestFileHelper();
        return type switch
        {
            TestFileType.Csv => file.CsvFileLocation,
            TestFileType.Json => file.JsonFileLocation,
            _ => throw new(nameof(type)),
        };
    }
    #endregion
}
internal enum TestFileType
{
    Json,
    Csv
}