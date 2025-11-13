using LeadPipe.Infrastructure.Services;
using LeadPipe.Infrastructure.Test.FileManipulationTests.FileHelpers;

namespace LeadPipe.Infrastructure.Test.FileManipulationTests;

public class CsvServiceTest
{
    #region Write Theory
    [
        Theory,
        InlineData("1234", "What's in a name", "2024-27-08T08:15"), // Since this inline data will be used to write to CSV, it only makes sense to have one data set
    ]
    #endregion
    #region Write
    public void CsvService_WritesToCsv(string id, string name, string dateTime)
    {
        // Assemble
        // Create the File name
        FileInfo fileName = new(TestFileHelper.AccessTestFile(TestFileType.Csv));

        // Create the actual object to be saved to file
        TestFile content = TestFileHelper.ParseStringToTestFile(id, name, dateTime, out int intDefault, out DateTime dtDefault, out int idResult, out DateTime dtResult);

        // Act
        CsvRwService.Write<TestFile, TestFileMap>(fileName, [content]);

        // Assert
        Assert.NotEqual(intDefault, content.Id); // The id does not equal the default -- otherwise, there was a parsing error
        Assert.NotEqual(dtDefault, content.DateTime); // The date time does not equal the default -- otherwise, there was a parsing error

        // The content should be equal to the result of the parse
        Assert.Equal(content.Id, idResult);
        Assert.Equal(content.DateTime, dtResult);
        Assert.Equal(content.Name, name);
    }
    #endregion

    #region Parse Fact
    [Fact]
    public void CsvService_ParsesFromCsv()
    {
        // Assemble
        // Create the file name
        FileInfo fileName = new(TestFileHelper.AccessTestFile(TestFileType.Csv));

        // Act
        var contents = CsvRwService.Parse<TestFile>(fileName);

        // Assert
        Assert.True(contents.IsSuccess);
        Assert.NotNull(contents.Value);
    }
    #endregion

    #region Append Theory
    [
        Theory, // Inline Data is raw Json
        InlineData("1234", "What's in a name", "2024-27-08T08:15"),
        InlineData("2345", "What's in a name", "2024-27-08T08:14"),
        InlineData("3456", "What's in a name", "2024-27-08T08:13"),
        InlineData("4567", "What's in a name", "2024-27-08T08:12"),
        InlineData("5678", "What's in a name", "2024-27-08T08:11"),
        InlineData("6789", "What's in a name", "2024-27-08T08:10"),
        InlineData("7890", "What's in a name", "2024-27-08T08:19"),
        InlineData("8901", "What's in a name", "2024-27-08T08:18"),
        InlineData("9012", "What's in a name", "2024-27-08T00:17"),
        InlineData("0123", "What's in a name", "2024-27-08T01:16"),
        InlineData("9876", "What's in a name", "2024-27-08T02:05"),
        InlineData("8765", "What's in a name", "2024-27-08T03:25"),
        InlineData("7654", "What's in a name", "2024-27-08T06:45"),
        InlineData("6543", "What's in a name", "2024-27-08T04:65"),
        InlineData("5432", "What's in a name", "2024-27-08T05:35"),
        InlineData("4321", "What's in a name", "2024-27-08T06:55"),
        InlineData("3210", "What's in a name", "2024-27-08T07:15"),
    ]
    #endregion
    #region Append
    public void CsvService_AppendsToCsv(string id, string name, string dateTime)
    {
        // Assemble
        FileInfo fileName = new(TestFileHelper.AccessTestFile(TestFileType.Csv));
        
        // Create the actual object to be saved to file
        TestFile content = TestFileHelper.ParseStringToTestFile(id, name, dateTime, out int intDefault, out DateTime dtDefault, out int idResult, out DateTime dtResult);

        // Act
        CsvRwService.Append<TestFile, TestFileMap>(fileName, [content]);

        // Assert
        // Assert
        Assert.NotEqual(intDefault, content.Id); // The id does not equal the default -- otherwise, there was a parsing error
        Assert.NotEqual(dtDefault, content.DateTime); // The date time does not equal the default -- otherwise, there was a parsing error

        // The content should be equal to the result of the parse
        Assert.Equal(content.Id, idResult);
        Assert.Equal(content.DateTime, dtResult);
        Assert.Equal(content.Name, name);
    }
    #endregion
}
