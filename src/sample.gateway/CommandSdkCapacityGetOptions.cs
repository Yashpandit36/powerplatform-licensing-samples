namespace sample.gateway
{
    using Microsoft.Extensions.Options;
    using sample.gateway.Discovery;

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
            var authentication = serviceProvider.GetRequiredService<IMicrosoftAuthentication>();
            var gatewayConfig = serviceProvider.GetRequiredService<IOptionsMonitor<GatewayConfig>>();
            var neptuneDiscovery = serviceProvider.GetRequiredService<INeptuneDiscovery>();

            var cmd = new CommandSdkCapacityGet(this, configuration, gatewayConfig, neptuneDiscovery, logger);
            var result = cmd.Run();
            return result;
        }
    }
}