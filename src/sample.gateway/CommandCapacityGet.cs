namespace sample.gateway;

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using Microsoft.Extensions.Options;
using sample.gateway.Discovery;
using sample.gateway.Tokens;

public class CommandCapacityGet : BaseCommand<CommandCapacityGetOptions>
{
    private readonly GatewayConfig _gatewayConfig;
    private readonly INeptuneDiscovery _neptuneDiscovery;
    private string _clientId;
    private Uri _authority;
    private IPublicClientApplication _clientApplication;

    public CommandCapacityGet(
        CommandCapacityGetOptions opts,
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
            // Neptune PPAPI GW Tenant routing URL
            string tenantUrl = _neptuneDiscovery.GetTenantEndpoint(Opts.TenantId);
            Uri gatewayTenantUri = new Uri($"https://{tenantUrl}");

            (bool flowControl, string value) = GetCapacityReport(gatewayTenantUri, Opts.TenantId);
            if (flowControl)
            {
                List<CapacityGetModel> results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CapacityGetModel>>(value);

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
    private (bool flowControl, string value) GetCapacityReport(Uri gatewayTenantUri, TenantId tenantId)
    {
        string gatewayResource = _neptuneDiscovery.GetTokenAudience();
        string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
        string gatewayAccessToken = OnAcquireUserToken(_clientApplication, _authority, _clientId, gatewayResource, tokenPrefix, "gateway").GetAwaiter().GetResult();
        if (string.IsNullOrWhiteSpace(gatewayAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for gateway.");
            return (flowControl: false, value: string.Empty);
        }

        bool requestOk = false;
        Uri requestUrl = new Uri(gatewayTenantUri, $"/licensing/tenantCapacity/currencyReports?api-version=2022-03-01-preview");
        string requestResponse = OnSendAsync(requestUrl.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Get, correlationId: Guid.NewGuid(), cancellationToken: CancellationToken.None);

        if (string.IsNullOrWhiteSpace(requestResponse))
        {
            TraceLogger.LogInformation($"Failed {requestUrl}.");
            TraceLogger.LogInformation($"Failed to retrieve currencyReports {tenantId}.");
        }
        else
        {
            TraceLogger.LogInformation($"Succeeded {requestUrl}.");
            TraceLogger.LogInformation($"currencyReports for {tenantId}: {requestResponse}");
            requestOk = true;
        }

        return (flowControl: requestOk, value: requestResponse);
    }

}