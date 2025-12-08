namespace sample.gateway;

using System.Collections.Generic;

public class CommandSdkCapacityGet : BaseCommand<CommandSdkCapacityGetOptions>
{
    private ServiceClient _apiDiscovery;

    public CommandSdkCapacityGet(
        CommandSdkCapacityGetOptions opts,
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider) : base(opts, configuration, logger, serviceProvider)
    {
    }

    public override void OnInit()
    {
        ServiceClientFactory apiClientFactory = new ServiceClientFactory();

        _clientId = PowershellClientId.ToString();
        _apiDiscovery = apiClientFactory.Create(_clientId);
    }

    public override int OnRun()
    {
        try
        {
            /// You need to be a Tenant Admin or an Environment Admin
            /// 
            List<CurrencyReportV2> reports = _apiDiscovery.Licensing.TenantCapacity.CurrencyReports
                .GetAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            return -1;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(ex.Message);
            return -1;
        }
    }

}