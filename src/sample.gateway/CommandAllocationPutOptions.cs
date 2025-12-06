namespace sample.gateway;

using Microsoft.Extensions.Options;
using sample.gateway.Discovery;
using sample.gateway.Tokens;

[Verb("CommandAllocationPut")]
public class CommandAllocationPutOptions : CommandOptions
{
    [Option("tenantId", Required = false, SetName = "AllParameterSets", HelpText = "Tenant Id for which token will be issued")]
    public string TenantId { get; set; }

    [Option(shortName: 'a', "action", Required = false, SetName = "AllParameterSets", HelpText = "Action to perform")]
    public CommandAllocationPutOptionsAction Action { get; set; } = CommandAllocationPutOptionsAction.DisableDrawFromTenantPool;

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        var authentication = serviceProvider.GetRequiredService<IMicrosoftAuthentication>();
        var gatewayConfig = serviceProvider.GetRequiredService<IOptionsMonitor<GatewayConfig>>();
        var neptuneDiscovery = serviceProvider.GetRequiredService<INeptuneDiscovery>();

        var cmd = new CommandAllocationPut(this, configuration, gatewayConfig, neptuneDiscovery, logger);
        var result = cmd.Run();
        return result;
    }
}