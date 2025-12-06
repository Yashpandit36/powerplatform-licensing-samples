namespace sample.gateway
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using sample.gateway.Discovery;

    public class CommandEvaluateEntitlement : BaseCommand<CommandEvaluateEntitlementOptions>
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IMicrosoftAuthentication _microsoftAuthentication;
        private readonly GatewayConfig _gatewayConfig;
        private readonly INeptuneDiscovery _neptuneDiscovery;
        private string _ClientId;
        private string _TenantId;
        private string _ServiceAuthRegionalCertSubjectName;

        public CommandEvaluateEntitlement(
            CommandEvaluateEntitlementOptions opts,
            IConfiguration configuration,
            ILogger logger,
            IOptionsMonitor<GatewayConfig> gatewayConfig,
            INeptuneDiscovery neptuneDiscovery,
            IMicrosoftAuthentication microsoftAuthentication) : base(opts, configuration, logger)
        {
            _gatewayConfig = gatewayConfig?.CurrentValue ?? throw new ArgumentNullException(nameof(gatewayConfig));
            _neptuneDiscovery = neptuneDiscovery ?? throw new ArgumentNullException(nameof(neptuneDiscovery));
            _microsoftAuthentication = microsoftAuthentication ?? throw new ArgumentNullException(nameof(microsoftAuthentication));
        }

        public override void OnInit()
        {
            _ClientId = _gatewayConfig.ClientId;
            _TenantId = _gatewayConfig.FirstPartyInfrastructureTenantId;
            _ServiceAuthRegionalCertSubjectName = _gatewayConfig.ServiceAuthRegionalCertSubjectName;

        }

        public override int OnRun()
        {
            string gatewayUrl = _neptuneDiscovery.GetGatewayEndpoint(Opts.UserTenantId, Opts.EnvironmentId);
            string gatewayAudience = Opts.Audience ?? _ClientId ?? string.Empty;

            // API Endpoint
            var clusterurl = $"https://{gatewayUrl}/licensing";

            if (!string.IsNullOrWhiteSpace(Opts.UserId))
            {
                clusterurl += $"/users/{Opts.UserId}";
            }

            clusterurl += $"/evaluateEntitlements?api-version=1";

            TraceLogger.LogInformation($"Gateway URL: {clusterurl}");
            TraceLogger.LogInformation($"Gateway Audience: {gatewayAudience}");

            if (Opts.WhatIf != true)
            {
                SendMessages(clusterurl, Opts.UserTenantId, Opts.NumberOfAttempts, gatewayAudience, CancellationToken.None);
            }

            return 0;
        }

        private void SendMessages(string url, string tenantId, int numRequests, string gatewayAudience, CancellationToken cancellationToken = default)
        {
            // Payload (JSON format)
            LicenseEvaluationRequestModel<EmptyLicenseEvaluationContextInfo> model = new LicenseEvaluationRequestModel<EmptyLicenseEvaluationContextInfo>()
            {
                EvaluationTypes = new List<LicenseEvaluationType>() { LicenseEvaluationType.Capacity },
                ProductCategory = ProductCategory.W365,
                Entitlements = new HashSet<string> { "W365APAYGO" }
            };
            string jsonPayload = JsonConvert.SerializeObject(model);
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Delay between requests (milliseconds)
            int delay = 10;

            // Acquire token for the gateway audience
            var gwtoken = AcquireToken(gatewayAudience, cancellationToken).GetAwaiter().GetResult();
            var accessToken = gwtoken.Token;

            for (int i = 0; i < numRequests; i++)
            {
                Guid correlationId = Guid.NewGuid();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Add("User-Agent", "neptune-sample");
                client.DefaultRequestHeaders.Add("x-ms-client-tenant-id", tenantId);
                client.DefaultRequestHeaders.Add("x-ms-correlation-id", correlationId.ToString());

                try
                {
                    var response = client.PostAsync(url, content, cancellationToken).Result;

                    TraceLogger.LogInformation($"Request {i + 1}: Status Code = {response.StatusCode}:  CorrelationId = {correlationId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        // We can add retry logic, transient fault handling, or logging here
                        string errorResponse = response.Content.ReadAsStringAsync(cancellationToken).Result;
                        TraceLogger.LogInformation($"Error Response: {errorResponse}");
                    }
                    else
                    {
                        var responseBody = response.Content.ReadAsStringAsync(cancellationToken).Result;
                        TraceLogger.LogInformation($"Response Message: {responseBody}");
                    }

                    TraceLogger.LogInformation("");
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    delay *= 2; // Exponential backoff on failure
                    TraceLogger.LogError(httpEx, $"Request {i + 1} failed: {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    delay *= 2; // Exponential backoff on failure
                    TraceLogger.LogError(ex, $"Request {i + 1} failed: {ex.Message}");
                }
                finally
                {
                    Task.Delay(delay).Wait(); // Wait before sending the next request
                    delay = 10; // Reset delay to initial value
                }
            }
        }

        private async Task<AccessToken> AcquireToken(string audience, CancellationToken cancellationToken = default)
        {

            var jwttoken = await _microsoftAuthentication
                .GetAccessTokenAsync(
                    resource: audience,
                    cancellation: cancellationToken,
                    authConfig: new MicrosoftAuthenticationConfig
                    {
                        TenantId = _TenantId,
                        ClientId = _ClientId,
                        ClientCertificateCommonName = _ServiceAuthRegionalCertSubjectName,
                        RegionName = "westus" // ESTS-REGION
                    },
                    scope: default,
                    useCachedTokenCredential: true);

            return jwttoken;
        }
    }
}