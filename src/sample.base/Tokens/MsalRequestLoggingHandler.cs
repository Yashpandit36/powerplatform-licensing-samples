namespace sample.gateway.Tokens;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

[ExcludeFromCodeCoverage]
internal class MsalRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger logger;

    /// <summary>
    /// The name of the User-Agent header.
    /// </summary>
    public const string UserAgentHeader = "x-ms-useragent";
    public static string UserAgentHeaderValue = $"powerplatform-licensing-samples/{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "UnknownVersion"} ({Environment.OSVersion.Platform})";

    public MsalRequestLoggingHandler(ILogger logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // client-request-id header matches the correlationId of all activities that deal with token acquisition
        request.Headers.TryGetValues("client-request-id", out IEnumerable<string> clientRequestId);
        logger.LogInformation($"MSAL Request: {request.Method} {request.RequestUri} - Client Request ID: {clientRequestId?.FirstOrDefault()}");

        // Set the User-Agent header
        if (!request.Headers.Contains(UserAgentHeader))
        {
            request.Headers.Add(UserAgentHeader, UserAgentHeaderValue);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}