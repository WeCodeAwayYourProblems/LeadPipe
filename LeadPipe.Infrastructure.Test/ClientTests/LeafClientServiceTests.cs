using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Translate;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace LeadPipe.Infrastructure.Test.ClientTests;

public class LeafClientServiceTests
{
    private readonly ILeafSettings _settings = Substitute.For<ILeafSettings>();
    private readonly IDtoToVo _dto = Substitute.For<IDtoToVo>();
    private readonly IJsonRwService _json = Substitute.For<IJsonRwService>();
    private readonly IHttpClientFactory _factory = Substitute.For<IHttpClientFactory>();
    private readonly IPlumbingRepository _repo = Substitute.For<IPlumbingRepository>();
    private readonly IFileService _file = Substitute.For<IFileService>();
    private readonly ILogger<LeafClientService> _logger = Substitute.For<ILogger<LeafClientService>>();

    private static HttpClient CreateHttpClient(object? obj, HttpStatusCode status = HttpStatusCode.OK)
        => new(new FakeHandler(obj, status));

    private LeafClientService CreateService(HttpClient client)
    {
        _settings.LeafConcurrentMax.Returns(5);
        _settings.LeafName.Returns("leaf");
        _settings.LeafThreadsEndpoint.Returns("http://example.com/threads");
        _settings.LeafMessagesEndpoint.Returns("/messages");

        _factory.CreateClient("leaf").Returns(client);

        return new LeafClientService(
            _settings,
            _dto,
            _json,
            _factory,
            _repo,
            _file,
            _logger
        );
    }

    [Fact]
    public void Update_AssignsMessagesByThread()
    {
        var leafs = new List<LeafDto>
        {
            new() { uuid = "A" },
            new() { uuid = "B" }
        };

        var msgs = new[]
        {
            Result.Success(new List<Message> { new() { thread = "A", message = "hi" } }),
            Result.Success(new List<Message> { new() { thread = "B", message = "yo" } })
        };

        var service = CreateService(CreateHttpClient(new()));

        var updated = service.Update(leafs, msgs);

        Assert.Equal("hi", updated[0].messages![0].message);
        Assert.Equal("yo", updated[1].messages![0].message);
    }

    [Fact]
    public async Task GetMessagesAsync_ReturnsMessages()
    {
        var sample = new List<Message>
        {
            new() { thread = "X", message = "test" }
        };

        var client = CreateHttpClient(sample);
        var service = CreateService(client);

        var leafs = new List<LeafDto> { new() { uuid = "X" } };

        var results = await service.GetMessagesAsync(leafs);

        Assert.Single(results);
        Assert.True(results[0].IsSuccess);
        Assert.Single(results[0].Value);
        Assert.Equal("X", results[0].Value[0].thread);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTranslatedPlumbing()
    {
        var rawDtos = new List<LeafDto>
        {
            new() { uuid = "1", messages = [] },
            new() { uuid = "2", messages = [] }
        };

        var httpClient = CreateHttpClient(rawDtos);
        var service = CreateService(httpClient);

        _file.GetLocalFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("raw.json");

        _json.WriteToFile(Arg.Any<FileInfo>(), Arg.Any<List<LeafDto>>())
            .Returns(Result.Success());

        _dto.Translate(Arg.Any<LeafDto>())
            .Returns(new Plumbing(new(PhoneNumber.Default), DateTimeOffset.MaxValue, null, Source.Test));

        var results = await service.GetAllAsync();

        Assert.True(results.IsSuccess);
        Assert.Equal(2, results.Value.Count);
    }
}

