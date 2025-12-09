namespace sample.gateway;

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;

public class CommandCapacityGet : BaseCommand<CommandCapacityGetOptions>
{
    public CommandCapacityGet(
        CommandCapacityGetOptions opts,
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider) : base(opts, configuration, logger, serviceProvider)
    {
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
            string tenantUrl = _neptuneDiscovery.GetGatewayEndpoint(Opts.TenantId);
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
        string gatewayAccessToken = OnAcquireUserToken(_clientId, gatewayResource, tokenPrefix, "gateway").GetAwaiter().GetResult();
        if (string.IsNullOrWhiteSpace(gatewayAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for gateway.");
            return (flowControl: false, value: string.Empty);
        }

        bool requestOk = false;
        Uri requestUrl = new Uri(gatewayTenantUri, $"/licensing/tenantCapacity/currencyReports?api-version=2022-03-01-preview");
        string requestResponse = OnSendAsync(requestUrl.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Get, correlationId: Guid.NewGuid(), cancellationToken: CancellationToken.None);

        if (!string.IsNullOrWhiteSpace(requestResponse))
        {
            TraceLogger.LogInformation($"Succeeded {requestUrl}.");
            TraceLogger.LogInformation($"currencyReports for {tenantId}: {requestResponse}");
            requestOk = true;
        }

        return (flowControl: requestOk, value: requestResponse);
    }

}