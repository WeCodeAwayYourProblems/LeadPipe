using Template.Infrastructure.JsonService;
using Template.Infrastructure.Test.FileManipulationTests.FileHelpers;

namespace Template.Infrastructure.Test.FileManipulationTests;

public class JsonServiceTest
{

    #region Write Theory
    [
        Theory, // Inline Data is raw Json values
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
    #region Write
    public void JsonService_WritesToFile(string id, string name, string dateTime)
    {
        // Assemble
        string jsonFile = TestFileHelper.AccessTestFile(TestFileType.Json);
        TestFile content = TestFileHelper.ParseStringToTestFile(id, name, dateTime, out int intDefault, out DateTime dtDefault, out int idResult, out DateTime dtResult);

        // Act
        JsonRw.SerializeToFile(jsonFile, [content]);

        // Assert
        Assert.NotEqual(intDefault, content.Id); // The id does not equal the default -- otherwise, there was a parsing error
        Assert.NotEqual(dtDefault, content.DateTime); // The date time does not equal the default -- otherwise, there was a parsing error

        // The content should be equal to the result of the parse
        Assert.Equal(content.Id, idResult);
        Assert.Equal(content.DateTime, dtResult);
        Assert.Equal(content.Name, name);
    }
    #endregion

    #region Deserialize Raw Theory
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
    #region Deserialize Raw
    public void JsonService_DeserializesRawJson(string id, string name, string dateTime)
    {
        // Assemble
        string rawJson = AssembleRawJson(id, name, dateTime);

        // Act
        List<TestFile> result = JsonRw.Deserialize<TestFile>(rawJson);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count != 0);
    }
    #endregion

    #region Read Fact
    [Fact]
    public void JsonService_ReadsFromFile()
    {
        // Assemble
        string jsonFile = TestFileHelper.AccessTestFile(TestFileType.Json);

        // Act
        List<TestFile> result = JsonRw.DeserializeFile<TestFile>(jsonFile);

        // Assert
        Assert.NotNull(result);
    }
    #endregion
    
    #region Private
    private static string AssembleRawJson(string id, string name, string dateTime)
    {
        const string q = "\"";
        const string i = "id";
        const string n = "name";
        const string d = "datetime";
        string rawJson = $"{{{q}{i}{q}:{q}{id}{q},{q}{n}{q}:{q}{name}{q},{q}{d}{q}:{q}{dateTime}{q}}}";
        return rawJson;
    }
    #endregion
}
