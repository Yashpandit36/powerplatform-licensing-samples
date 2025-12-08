namespace sample.gateway.Tokens;

using System.Net.Http;
using Microsoft.Identity.Client;

public sealed class MsalRequestHttpClientFactory : IMsalHttpClientFactory, IDisposable
{
    private readonly ILogger<MsalRequestHttpClientFactory> logger;
    private HttpClient httpClient;

    /// <summary>
    /// Creates a new instance of <see cref="MsalRequestHttpClientFactory"/> to be shared across all CCAs.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public MsalRequestHttpClientFactory(ILoggerFactory loggerFactory)
    {

        this.logger = loggerFactory.CreateLogger<MsalRequestHttpClientFactory>();

        HttpClientHandler innerHandler =
#if NETCOREAPP2_1_OR_GREATER
            new HttpClientHandler()
            {
                UseCookies = false,
                CheckCertificateRevocationList = true
            };
#elif NETFRAMEWORK
            new WinHttpHandler
            {
                CookieUsePolicy = CookieUsePolicy.IgnoreCookies,
                CheckCertificateRevocationList = true
            };
#else
#error Review code for NETSTANDARD
#endif

        this.httpClient = new HttpClient(new MsalRequestLoggingHandler(this.logger)
        {
            InnerHandler = innerHandler
        });
    }

    public HttpClient GetHttpClient() => this.httpClient;

    /// <inheritdoc />
    public void Dispose()
    {
        this.httpClient?.Dispose();
        this.httpClient = null;
    }
}