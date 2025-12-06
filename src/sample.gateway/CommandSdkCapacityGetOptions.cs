namespace sample.gateway;

using Microsoft.Extensions.Options;
using sample.gateway.Discovery;
using sample.gateway.Tokens;

[Verb("CommandSdkCapacityGet")]
public class CommandSdkCapacityGetOptions : CommandOptions
{
    [Option("tenantId", Required = false, SetName = "AllParameterSets", HelpText = "Tenant Id for which token will be issued")]
    public string TenantId { get; set; }

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        IMicrosoftAuthentication authentication = serviceProvider.GetRequiredService<IMicrosoftAuthentication>();
        IOptionsMonitor<GatewayConfig> gatewayConfig = serviceProvider.GetRequiredService<IOptionsMonitor<GatewayConfig>>();
        INeptuneDiscovery neptuneDiscovery = serviceProvider.GetRequiredService<INeptuneDiscovery>();

        var cmd = new CommandSdkCapacityGet(this, configuration, gatewayConfig, neptuneDiscovery, logger);
        var result = cmd.Run();
        return result;
    }
}