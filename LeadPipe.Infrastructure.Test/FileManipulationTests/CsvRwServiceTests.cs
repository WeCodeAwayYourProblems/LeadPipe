using CSharpFunctionalExtensions;
using CsvHelper.Configuration;
using LeadPipe.Infrastructure.Service;
using Org.BouncyCastle.Asn1.Mozilla;

namespace LeadPipe.Infrastructure.Test.FileManipulationTests;

public sealed class CsvRwServiceTests : IDisposable
{
    private readonly CsvRwService _service;
    private readonly string _tempDir;

    public CsvRwServiceTests()
    {
        _service = new CsvRwService();
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private FileInfo GetTempFile(string name = "test.csv")
    {
        return new FileInfo(Path.Combine(_tempDir, name));
    }

    public class TestRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class DefaultClassMap : ClassMap<TestRecord>
    {
        public DefaultClassMap()
        {
            int index = 0;
            Map(m => m.Id).Index(index++);
            Map(m=>m.Name).Index(index++);
        }
    }

    [Fact]
    public void Write_And_ReadFile_Should_Work()
    {
        var file = GetTempFile();
        var records = new List<TestRecord>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        };

        var writeResult = _service.Write(records, file);
        Assert.True(writeResult.IsSuccess);

        var readResult = _service.ReadFile<TestRecord>(file);
        Assert.True(readResult.IsSuccess);
        Assert.Equal(2, readResult.Value.Count);
        Assert.Equal("Alice", readResult.Value[0].Name);
        Assert.Equal("Bob", readResult.Value[1].Name);
    }

    [Fact]
    public async Task WriteAsync_And_ReadFileAsync_Should_Work()
    {
        var file = GetTempFile();
        var records = new List<TestRecord>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        };

        var writeResult = await _service.WriteAsync(records, file);
        Assert.True(writeResult.IsSuccess);

        var readResult = await _service.ReadFileAsync<TestRecord>(file);
        Assert.True(readResult.IsSuccess);
        Assert.Equal(2, readResult.Value.Count);
        Assert.Equal("Alice", readResult.Value[0].Name);
    }

    [Fact]
    public void Append_Should_Add_Records()
    {
        var file = GetTempFile();
        var firstBatch = new List<TestRecord> { new() { Id = 1, Name = "Alice" } };
        var secondBatch = new List<TestRecord> { new() { Id = 2, Name = "Bob" } };

        _service.Write(firstBatch, file);
        var appendResult = _service.Append<TestRecord, DefaultClassMap<TestRecord>>(file, secondBatch);
        Assert.True(appendResult.IsSuccess);

        var readResult = _service.ReadFile<TestRecord>(file);
        Assert.True(readResult.IsSuccess);
        Assert.Equal(2, readResult.Value.Count);
    }

    [Fact]
    public void ReadFile_Should_Return_Failure_If_File_Does_Not_Exist()
    {
        var file = GetTempFile("nonexistent.csv");
        var result = _service.ReadFile<TestRecord>(file);
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to perform the following action", result.Error);
    }

    [Fact]
    public async Task ReadFileAsync_Should_Return_Failure_If_File_Does_Not_Exist()
    {
        var file = GetTempFile("nonexistent.csv");
        var result = await _service.ReadFileAsync<TestRecord>(file);
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to perform the following action", result.Error);
    }

    [Fact]
    public async Task ReadFileAsync_Should_Respect_Cancellation()
    {
        var file = GetTempFile();
        var records = Enumerable.Range(1, 100).Select(i => new TestRecord { Id = i, Name = $"Name{i}" }).ToList();
        _service.Write(records, file);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _service.ReadFileAsync<TestRecord>(file, cts.Token));
    }

    [Fact]
    public async Task WriteAsync_Should_Respect_Cancellation()
    {
        var file = GetTempFile();
        var records = Enumerable.Range(1, 100).Select(i => new TestRecord { Id = i, Name = $"Name{i}" }).ToList();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Result result = await _service.WriteAsync(records, file, cts.Token);
        // WriteAsync internally does not throw for cancellation but flush might
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Write_Should_Handle_Empty_List()
    {
        var file = GetTempFile();
        var emptyList = new List<TestRecord>();
        var result = _service.Write(emptyList, file);
        Assert.True(result.IsSuccess);

        var readResult = _service.ReadFile<TestRecord>(file);
        Assert.True(readResult.IsSuccess);
        Assert.Empty(readResult.Value);
    }

}
