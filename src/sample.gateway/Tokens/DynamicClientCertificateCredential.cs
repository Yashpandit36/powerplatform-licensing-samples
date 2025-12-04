namespace sample.gateway.Tokens
{
    using Microsoft.Identity.Client;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using MICAuthenticationResult = Microsoft.Identity.Client.AuthenticationResult;
    using MICLogLevel = Microsoft.Identity.Client.LogLevel;

    [ExcludeFromCodeCoverage]
    internal class DynamicClientCertificateCredential : TokenCredential
    {
        private readonly ILogger logger;
        private readonly IMsalHttpClientFactory msalHttpClientFactory;
        private readonly MicrosoftAuthenticationConfig config;
        private IConfidentialClientApplication ConfidentialClientApplication;

        public DynamicClientCertificateCredential(ILogger logger, IMsalHttpClientFactory msalHttpClientFactory, MicrosoftAuthenticationConfig config)
        {

            this.config = config;
            this.logger = logger;
            this.msalHttpClientFactory = msalHttpClientFactory;

            var clientCertificate = this.config.Certificate;

            if (clientCertificate == null && string.IsNullOrEmpty(this.config.ClientCertificateCommonName))
            {
                throw new InvalidOperationException("Either ClientCertificate or ClientCertificateCommonName must be provided in the configuration.");
            }

            using (X509Store store = new X509Store(this.config.CertificateStoreName, this.config.CertificateStoreLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                // Find certificate by subject name
                X509Certificate2Collection certCollection = store.Certificates
                    .Find(X509FindType.FindBySubjectName, config.ClientCertificateCommonName, validOnly: false);

                clientCertificate = certCollection.FirstOrDefault(fn => fn.NotAfter >= DateTime.UtcNow);
            }

            var authoritySuffix = config.UseMultiTenantCredential == true
                ? "common"
                : this.config.AuthorityHost.AbsolutePath.ToLower().Contains(config.TenantId?.ToLower()) // Fallback in case consumer-configured AuthorityHost already contains the tenant id.
                    ? string.Empty
                    : config.TenantId;

            var authority = $"{this.config.AuthorityHost}{authoritySuffix}/";

            var ccaBuilder = ConfidentialClientApplicationBuilder
                .Create(this.config.ClientId)
                // config.ValidateForCertificateAuth() ensures that TenantId is a valid guid
                .WithAuthority(authority, validateAuthority: true)
                .WithCertificate(clientCertificate)
                // see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/High-availability#httpclient
                .WithHttpClientFactory(this.msalHttpClientFactory)
                // for telemetry purposes
                .WithClientName($"{nameof(DynamicClientCertificateCredential)}:{authoritySuffix}:{this.config.ClientId}")
                // see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/High-availability#use-the-token-cache
                .WithLegacyCacheCompatibility(false);

            if (this.config.EnableMsalInternalLogging)
            {
                ccaBuilder.WithLogging(
                    loggingCallback: this.LoggingCallback,
                    logLevel: MICLogLevel.Info,
                    enablePiiLogging: false);
            }

            ccaBuilder.WithAzureRegion(config.RegionName);

            ConfidentialClientApplication = ccaBuilder.Build();
        }

        /// <inheritdoc />
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            // https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2012
            return this.GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var authResult = await this.AuthenticateAsync(requestContext, cancellationToken);
            return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
        }

        /// <summary>
        /// Authenticates a caller against a given request context.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task tracking token acquisition execution.</returns>
        protected async Task<MICAuthenticationResult> AuthenticateAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return await this.AuthenticateAsync(requestContext, cancellationToken, AuthenticateAsyncFunc);

            // Helper to acquire access token
            Task<MICAuthenticationResult> AuthenticateAsyncFunc(IConfidentialClientApplication confidentialClientApplication, TokenRequestContext context, CancellationToken cancellation, bool forceRefresh)
            {
                var acquireTokenBuilder = confidentialClientApplication
                    .AcquireTokenForClient(context.Scopes)
                    .WithCorrelationId(Guid.TryParse(context.ParentRequestId, out Guid requestId) ? requestId : Guid.Empty)
                    .WithSendX5C(true) // enables SAN+I auth.
                    .WithForceRefresh(forceRefresh);

                if (this.config.UseMultiTenantCredential == true)
                {
                    acquireTokenBuilder = acquireTokenBuilder.WithTenantId(context.TenantId);
                }

                return acquireTokenBuilder.ExecuteAsync(cancellation);
            }
        }

        /// <summary>
        /// Authenticates a caller against a given request context.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="authenticateFunc">The authentication function.</param>
        /// <returns>Task tracking token acquisition execution.</returns>
        protected async Task<MICAuthenticationResult> AuthenticateAsync(TokenRequestContext requestContext, CancellationToken cancellationToken, Func<IConfidentialClientApplication, TokenRequestContext, CancellationToken, bool, Task<MICAuthenticationResult>> authenticateFunc)
        {
            requestContext = new TokenRequestContext(
                scopes: requestContext.Scopes,
                parentRequestId: requestContext.ParentRequestId,
                tenantId: requestContext.TenantId);

            // Second, use the app to get the token.
            var confidentialClient = this.ConfidentialClientApplication;

            var result = await authenticateFunc(confidentialClient, requestContext, cancellationToken, false);

            return result;
        }

        /// <summary>
        /// The logging callback to facilitate logging from within MSAL.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message.</param>
        /// <param name="contrainsPii">Boolean whether message contains PII. No PII may be logged.</param>
        private void LoggingCallback(MICLogLevel logLevel, string message, bool contrainsPii)
        {
            // Make sure we do not log PII
            // Even though this is running as a ConfidentialClientApplication, it is better safe than sorry when it comes to PII.
            string messageEntry = contrainsPii ? "Passed message contains PII - will not be logged." : message;
            switch (logLevel)
            {
                case MICLogLevel.Verbose:
                    this.logger.LogDebug(messageEntry);
                    break;
                case MICLogLevel.Warning:
                    this.logger.LogWarning(messageEntry);
                    break;
                case MICLogLevel.Error:
                    this.logger.LogError(messageEntry);
                    break;
                case MICLogLevel.Info:
                default:
                    this.logger.LogInformation(messageEntry);
                    break;
            }
        }
    }
}