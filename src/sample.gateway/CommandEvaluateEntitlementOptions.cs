namespace sample.gateway
{
    using Microsoft.Extensions.Options;
    using sample.gateway.Discovery;

    /// <summary>
    /// Represents the options for the Entitlement check command, which uses the gateway cluster or island tenant/environment cluster routes
    /// </summary>
    /// <example>
    /// 
    /// Check Tenant Entitlement using the Tenant Island Route
    /// EvaluateEntitlement --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 
    /// 
    /// Check Tenant Entitlement using the Gateway Route
    /// EvaluateEntitlement --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --gateway true
    /// 
    /// Check Environment Entitlement using the Environment Route
    /// EvaluateEntitlement --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0 --environmentId 4b62a25e-1c3d-e2bc-9270-307db9f15b00
    /// 
    /// Check Environment Entitlement using the Gateway Route
    /// EvaluateEntitlement --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0 --environmentId 4b62a25e-1c3d-e2bc-9270-307db9f15b00 --gateway true
    /// 
    /// </example> 
    [Verb("EvaluateEntitlement", HelpText = "This will use credentials from certificates to assert JWT tokens")]
    public class CommandEvaluateEntitlementOptions : CommandOptions
    {
        [Option("audience", Required = false, HelpText = "The audience for which to acquire a token")]
        public string Audience { get; set; }

        [Option("tenantId", Required = false, SetName = "AllParameterSets", HelpText = "Tenant Id for downstream user tenant.")]
        public string UserTenantId { get; set; } = default(string);

        [Option("environmentId", Required = false, SetName = "AllParameterSets", HelpText = "Environment Id for downstream user tenant.")]
        public string EnvironmentId { get; set; } = default(string);

        [Option("userId", Required = false, SetName = "AllParameterSets", HelpText = "User Id for which token will be issued.")]
        public string UserId { get; set; } = default(string);

        public int RunGenerateAndReturnExitCode(
            IConfiguration configuration,
            ILogger logger,
            IServiceProvider serviceProvider)
        {
            var authentication = serviceProvider.GetRequiredService<IMicrosoftAuthentication>();
            var gatewayConfig = serviceProvider.GetRequiredService<IOptionsMonitor<GatewayConfig>>();
            var neptuneDiscovery = serviceProvider.GetRequiredService<INeptuneDiscovery>();

            var cmd = new CommandEvaluateEntitlement(this, configuration, logger, gatewayConfig, neptuneDiscovery, authentication);
            var result = cmd.Run();
            return result;
        }
    }
}