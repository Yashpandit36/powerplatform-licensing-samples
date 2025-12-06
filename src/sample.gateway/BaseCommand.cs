namespace sample.gateway
{
    using System.Net.Http;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class BaseCommand<T> where T : ICommandOptions
    {
        public IConfiguration Configuration { get; }
        public virtual T Opts { get; }
        public ILogger TraceLogger { get; }
        private static readonly HttpClient client = new();

        /// <summary>
        /// Microsoft Powershell
        /// </summary>
        internal Guid PowershellClientId { get; } = Guid.Parse("1950a258-227b-4e31-a9cf-717495945fc2");

        protected BaseCommand(T opts, IConfiguration configuration, ILogger logger)
        {
            Opts = opts ?? throw new ArgumentNullException(nameof(opts), "CommonOptions object is required.");
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "IAppSettings is required.");
            LoggerAvailable = false;
            TraceLogger = logger;
        }

        public int Run()
        {
            int result = -1;
            OnInit();

            try
            {
                result = OnRun();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Cmdlet Exception {0}", ex.Message);
                LogError(new Exception("Failed to run commandline switch."), $"OpenError");
            }
            finally
            {
                OnEnd();
            }

            return result;
        }

        public abstract void OnInit();

        public abstract int OnRun();

        public virtual void OnEnd() { }

        /// <summary>
        /// the logger is available
        /// </summary>
        internal bool LoggerAvailable { get; private set; }

        protected virtual void WriteConsole(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Log: ERROR
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        protected virtual void LogError(Exception ex, string message, params object[] args)
        {
            if (LoggerAvailable)
            {
                TraceLogger.LogError(ex, message, args);
            }

            if (string.IsNullOrEmpty(message))
            {
                System.Diagnostics.Trace.TraceError("Exception: {0}", ex.Message);
            }
            else
            {
                System.Diagnostics.Trace.TraceError(message, args);
            }
        }

        /// <summary>
        /// Log: DEBUG
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        protected virtual void LogDebugging(string message, params object[] args)
        {
            if (LoggerAvailable)
            {
                TraceLogger.LogDebug(message, args);
            }
            System.Diagnostics.Trace.TraceInformation(message, args);
        }

        /// <summary>
        /// Writes a warning message to the cmdlet and logs to directory
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        protected virtual void LogWarning(string message, params object[] args)
        {
            if (LoggerAvailable)
            {
                TraceLogger.LogWarning(message, args);
            }
            System.Diagnostics.Trace.TraceWarning(message, args);
        }

        /// <summary>
        /// Log: VERBOSE
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        protected virtual void LogVerbose(string message, params object[] args)
        {
            if (LoggerAvailable)
            {
                TraceLogger.LogInformation(message, args);
            }
            System.Diagnostics.Trace.TraceInformation(message, args);
        }

        /// <summary>
        /// Sends an HTTP GET request
        /// </summary>
        /// <param name="url">The gateway endpoint to be requested.</param>
        /// <param name="tenantId">The tenant id should be from your EntraId.  This value should match with your token TID.</param>
        /// <param name="accessToken">The access token claimed in previous steps.</param>
        /// <param name="httpMethod">The HTTP method to be used for the request.</param>
        /// <param name="requestBody">The request body to be sent with the request. Leave empty if this is a GET.</param>
        /// <param name="correlationId">The correlation id associated with the requests.  This assists in tracing.</param>
        /// <param name="cancellationToken">The cancellation token to handle the thread requests.</param>
        /// <returns></returns>
        internal virtual string OnSendAsync(
            string url,
            string tenantId,
            string accessToken,
            HttpMethod httpMethod,
            string requestBody = "",
            Guid? correlationId = default,
            CancellationToken cancellationToken = default)
        {
            correlationId ??= Guid.NewGuid();

            HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("User-Agent", "neptune-sample");
            request.Headers.Add("x-ms-client-tenant-id", tenantId);
            request.Headers.Add("x-ms-correlation-id", correlationId.ToString());

            try
            {
                if(!string.IsNullOrWhiteSpace(requestBody))
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                }

                var response = client.SendAsync(request, cancellationToken).Result;

                TraceLogger.LogInformation("Request Status Code = {StatusCode}:  CorrelationId = {CorrelationId}", response.StatusCode, correlationId);

                if (!response.IsSuccessStatusCode)
                {
                    // We can add retry logic, transient fault handling, or logging here
                    string errorResponse = response.Content.ReadAsStringAsync(cancellationToken).Result;
                    TraceLogger.LogInformation("Error Response: {ErrorResponse}", errorResponse);
                }
                else
                {
                    string responseBody = response.Content.ReadAsStringAsync(cancellationToken).Result;
                    return responseBody;
                }

            }
            catch (HttpRequestException httpEx)
            {
                TraceLogger.LogError(httpEx, "Request failed: {Message}", httpEx.Message);
            }
            catch (Exception ex)
            {
                TraceLogger.LogError(ex, "Request failed: {Message}", ex.Message);
            }
            finally
            {
            }

            return string.Empty;
        }

        /// <summary>
        /// This method acquires a user token interactively using MSAL.NET's PublicClientApplication.
        /// Please note that this method is only supported on Windows platforms due to storing the token in the user roaming store.
        /// </summary>
        /// <param name="clientApplication">The MSAL client to aquire tokens.</param>
        /// <param name="authority">The authority typically login.microsoftonline.com/common or login.microsoft.com/<tenantid>.</tenantid></param>
        /// <param name="clientId">The application registered in the EntraID for the <tenantid> specified in the authority.</param>
        /// <param name="resource">The 'scope' or 'audience' for which the token will be claimed.</param>
        /// <param name="tokenPrefix">Used to differentiate token storage.</param>
        /// <param name="tokenResource"></param>
        /// <returns>A cached access token.</returns>
        [SupportedOSPlatform("windows")]
        internal virtual async Task<string> OnAcquireUserToken(
            IPublicClientApplication clientApplication,
            Uri authority,
            string clientId,
            string resource,
            string tokenPrefix,
            string tokenResource)
        {
            var scopes = new[] { resource.GetScopeEnsureResourceTrailingSlash() };
            var resourceScoped = $"{tokenPrefix}-{tokenResource}".ToLowerInvariant();

            // The consent URL is not used in this method, but it can be useful for debugging or manual consent
            // We need the user to provide consent once
            TraceLogger.LogInformation($"Consent URL: {authority}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read");

            var token = TokenStorage.LoadToken(resourceScoped);

            if (token == null || token.ExpirationUtc <= DateTime.UtcNow)
            {
                // Token missing or expired: Get new token from server!
                var authenticationResult = await clientApplication
                    .AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
                string accessToken = authenticationResult.AccessToken;

                TokenStorage.SaveToken(new TokenInfo
                {
                    AccessToken = accessToken,
                    ExpirationUtc = authenticationResult.ExpiresOn
                },
                resourceScoped);

                return accessToken;
            }
            else
            {
                TraceLogger.LogInformation("Using cached authentication result.");
                return token.AccessToken;
            }
        }

        /// <summary>
        /// Example SSRF prevention: Make sure nextLink is on expected host and scheme
        /// </summary>
        /// <param name="baseUri">the root domain for the initial request.</param>
        /// <param name="nextLink">the nextLink from paged results, to validate.</param>
        /// <returns></returns>
        internal static bool IsSafeNextLink(Uri baseUri, string nextLink)
        {
            if (Uri.TryCreate(nextLink, UriKind.Absolute, out Uri uri))
            {
                // Only allow host & scheme you trust
                if (uri.Host == baseUri.Host && uri.Scheme == baseUri.Scheme)
                {
                    // (Optional) additional path checks
                    return true;
                }
            }
            return false;
        }
    }
}