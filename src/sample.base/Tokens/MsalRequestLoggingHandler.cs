namespace sample.gateway.Tokens;

using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

[ExcludeFromCodeCoverage]
internal class MsalRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger logger;

    public MsalRequestLoggingHandler(ILogger logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // client-request-id header matches the correlationId of all activities that deal with token acquisition
        request.Headers.TryGetValues("client-request-id", out var clientRequestId);
        logger.LogInformation($"MSAL Request: {request.Method} {request.RequestUri} - Client Request ID: {clientRequestId?.FirstOrDefault()}");
        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}