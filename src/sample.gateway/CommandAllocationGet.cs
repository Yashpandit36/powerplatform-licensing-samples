namespace sample.gateway
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Runtime.Versioning;
    using System.Threading;
    using Microsoft.Extensions.Options;
    using sample.gateway.Discovery;

    public class CommandAllocationGet : BaseCommand<CommandAllocationGetOptions>
    {
        private static readonly HttpClient client = new();
        private ServiceClient _apiDiscovery;
        private readonly GatewayConfig _gatewayConfig;
        private readonly INeptuneDiscovery _neptuneDiscovery;
        private string _clientId;
        private Uri _authority;
        private IPublicClientApplication _clientApplication;

        public CommandAllocationGet(
            CommandAllocationGetOptions opts,
            IConfiguration configuration,
            IOptionsMonitor<GatewayConfig> gatewayConfig,
            INeptuneDiscovery neptuneDiscovery,
            ILogger logger) : base(opts, configuration, logger)
        {
            _gatewayConfig = gatewayConfig?.CurrentValue ?? throw new ArgumentNullException(nameof(gatewayConfig));
            _neptuneDiscovery = neptuneDiscovery ?? throw new ArgumentNullException(nameof(neptuneDiscovery));
        }

        public override void OnInit()
        {
            _clientId = PowershellClientId.ToString();
            ServiceClientFactory apiClientFactory = new ServiceClientFactory();
            _apiDiscovery = apiClientFactory.Create(_clientId);

            _authority = new Uri(_gatewayConfig.AuthenticationEndpoint.GetScopeEnsureResourceTrailingSlash(Opts.TenantId));

            // Will will use a Public Client to obtain tokens interactively
            _clientApplication = PublicClientApplicationBuilder
              .Create(_clientId)
              .WithAuthority(_authority.ToString(), validateAuthority: true)
              .WithDefaultRedirectUri()
              .WithInstanceDiscovery(enableInstanceDiscovery: true)
              .Build();
        }

        // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
        [SupportedOSPlatform("windows")]
        public override int OnRun()
        {
            try
            {
                /// You need to be a Tenant Admin or an Environment Admin
                /// 
                List<CurrencyReportV2> reports = _apiDiscovery.Licensing.TenantCapacity.CurrencyReports.GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                // Neptune PPAPI GW Tenant routing URL
                string tenantUrl = _neptuneDiscovery.GetTenantEndpoint(Opts.TenantId);
                Uri gatewayTenantUri = new Uri($"https://{tenantUrl}");

                (bool flowControl, string value) = GetEnvironmentAllocation(gatewayTenantUri, Opts.TenantId, Opts.EnvironmentId);
                if (flowControl)
                {
                    TenantCapacityDetailsModel results = Newtonsoft.Json.JsonConvert.DeserializeObject<TenantCapacityDetailsModel>(value);

                    return 0;
                }

                return -1;
            }
            catch (Exception ex)
            {
                TraceLogger.LogError(ex.Message);
                return -1;
            }
        }

        // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
        [SupportedOSPlatform("windows")]
        private (bool flowControl, string value) GetEnvironmentAllocation(Uri gatewayTenantUri, TenantId tenantId, EnvironmentId environmentId)
        {
            string gatewayResource = _neptuneDiscovery.GetTokenAudience();
            string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
            string tokenSuffix = "gateway";

            string gatewayAccessToken = OnAcquireUserToken(_clientApplication, _authority, _clientId, gatewayResource, tokenPrefix, tokenSuffix).GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(gatewayAccessToken))
            {
                TraceLogger.LogError("Failed to acquire token for gateway.");
                return (flowControl: false, value: string.Empty);
            }

            bool allocationsOk = false;
            Uri allocationsUrl = new Uri(gatewayTenantUri, $"/licensing/environments/{environmentId}/allocations?api-version=2022-03-01-preview");
            string allocationsResponse = OnSendAsync(allocationsUrl.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Get, correlationId: Guid.NewGuid(), cancellationToken: CancellationToken.None);
            if (string.IsNullOrWhiteSpace(allocationsResponse))
            {
                TraceLogger.LogInformation($"Failed {allocationsUrl}.");
                TraceLogger.LogInformation($"Failed to retrieve allocations {environmentId}.");
            }
            else
            {
                TraceLogger.LogInformation($"Succeeded {allocationsUrl}.");
                TraceLogger.LogInformation($"Allocations for {environmentId}: {allocationsResponse}");
                allocationsOk = true;
            }

            return (flowControl: allocationsOk, value: allocationsResponse);
        }

    }
}