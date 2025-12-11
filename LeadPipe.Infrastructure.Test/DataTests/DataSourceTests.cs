using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Interfaces.Translate;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.DataTests;

public class DataSourceTests
{
    #region FileDataSource Tests

    [Theory]
    [InlineData(".csv")]
    [InlineData(".json")]
    public async Task CalliFileDataSource_LoadAsync_ReturnsFromCorrectReader(string extension)
    {
        // Arrange
        string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + extension);
        var tempFile = new FileInfo(tempFilePath);

        var csvMock = Substitute.For<ICsvRwService>();
        var jsonMock = Substitute.For<IJsonRwService>();
        var loggerMock = Substitute.For<ILogger<CalliFileDataSource>>();

        List<CalliDto> dtoList = [new()];
        if (extension == ".csv")
            csvMock.ReadFile<CalliDto>(Arg.Any<FileInfo>()).Returns(Result.Success(dtoList));
        else
            jsonMock.ReadFile<CalliDto>(Arg.Any<FileInfo>()).Returns(Result.Success(dtoList));

        var ds = new CalliFileDataSource(tempFile, csvMock, jsonMock, loggerMock);

        // Act
        var result = await ds.LoadAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dtoList, result.Value);

        // Cleanup
        if (tempFile.Exists) File.Delete(tempFile.FullName);
    }

    #endregion

    #region LabDataSource Tests

    [Fact]
    public async Task LabDataSource_LoadAsync_ReturnsTranslatedDtos_OnSuccess()
    {
        // Arrange
        var labServiceMock = Substitute.For<ILabService>();
        var voToDtoMock = Substitute.For<IVoToDto<Plumbing, LabDto>>();

        var plumbings = new List<Plumbing> { new Plumbing(new(PhoneNumber.Default), DateTimeOffset.MaxValue, string.Empty, Source.Test) };
        var labDtos = new List<LabDto> { new LabDto() };

        labServiceMock.GetLabsAsync().Returns(Task.FromResult(Result.Success(plumbings)));
        voToDtoMock.Translate(Arg.Any<Plumbing>()).Returns(labDtos[0]);

        var ds = new LabDataSource(labServiceMock, voToDtoMock);

        // Act
        var result = await ds.LoadAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(labDtos.Count, result.Value.Count);
        await labServiceMock.Received(1).GetLabsAsync();
    }

    [Fact]
    public async Task LabDataSource_LoadAsync_ReturnsFailure_OnRepoFailure()
    {
        // Arrange
        var labServiceMock = Substitute.For<ILabService>();
        var voToDtoMock = Substitute.For<IVoToDto<Plumbing, LabDto>>();

        labServiceMock.GetLabsAsync().Returns(Task.FromResult(Result.Failure<List<Plumbing>>("error")));

        var ds = new LabDataSource(labServiceMock, voToDtoMock);

        // Act
        var result = await ds.LoadAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
    }

    #endregion

    #region LeafDataSource Tests

    [Fact]
    public async Task LeafDataSource_LoadAsync_ReturnsTranslatedDtos_OnSuccess()
    {
        // Arrange
        var leafServiceMock = Substitute.For<ILeafService>();
        var voToDtoMock = Substitute.For<IVoToDto<Plumbing, LeafDto>>();

        var plumbings = new List<Plumbing> { new Plumbing(new(PhoneNumber.Default), DateTimeOffset.MaxValue, string.Empty, Source.Test) };
        var leafDtos = new List<LeafDto> { new LeafDto() };

        leafServiceMock.GetAllAsync().Returns(Task.FromResult(Result.Success(plumbings)));
        voToDtoMock.Translate(Arg.Any<Plumbing>()).Returns(leafDtos[0]);

        var ds = new LeafDataSource(leafServiceMock, voToDtoMock);

        // Act
        var result = await ds.LoadAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leafDtos.Count, result.Value.Count);
        await leafServiceMock.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task LeafDataSource_LoadAsync_ReturnsFailure_OnRepoFailure()
    {
        // Arrange
        var leafServiceMock = Substitute.For<ILeafService>();
        var voToDtoMock = Substitute.For<IVoToDto<Plumbing, LeafDto>>();

        leafServiceMock.GetAllAsync().Returns(Task.FromResult(Result.Failure<List<Plumbing>>("error")));

        var ds = new LeafDataSource(leafServiceMock, voToDtoMock);

        // Act
        var result = await ds.LoadAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
    }

    #endregion
}
