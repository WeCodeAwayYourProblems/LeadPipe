using System.Net;
using System.Net.Http.Json;

namespace LeadPipe.Infrastructure.Test.ClientTests;

internal class FakeHandler(object? responseObj, HttpStatusCode status) : HttpMessageHandler
{
    private readonly object? _responseObj = responseObj;
    private readonly HttpStatusCode _status = status;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var msg = new HttpResponseMessage(_status);

        if (_responseObj != null)
            msg.Content = JsonContent.Create(_responseObj);

        return Task.FromResult(msg);
    }
}
