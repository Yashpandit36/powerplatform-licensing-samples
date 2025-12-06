namespace sample.gateway;

using Microsoft.Extensions.Options;
using sample.gateway.Discovery;
using sample.gateway.Tokens;

/// <summary>
/// Represents the options for the "Role Assignment" evaluation command which uses the Tenant, Environment, or Gateway routes to check role assignments.
/// </summary>
/// <remarks>
/// This class provides configuration options for acquiring tokens and performing role
/// assignments.  It includes parameters such as audience, client ID, tenant ID, and user ID, which are used to 
/// specify the context and scope of the operation.
/// </remarks>
/// <example>
/// 
/// Check Role Assignment using the Tenant Island Route
/// EvaluateRoleAssignment --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --userId b2804446-d441-422f-99d4-ca4d20f84d99
/// 
/// Check Role Assignment using the Gateway Route
/// EvaluateRoleAssignment --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --userId b2804446-d441-422f-99d4-ca4d20f84d99 --gateway true
/// 
/// </example>
[Verb("EvaluateRoleAssignment", HelpText = "This will use credentials from certificates to assert JWT tokens")]
public class CommandEvaluateRoleAssignmentOptions : CommandOptions
{
    [Option("audience", Required = false, HelpText = "The audience for which to acquire a token")]
    public string Audience { get; set; }

    [Option("tenantId", Required = true, SetName = "AllParameterSets", HelpText = "Tenant Id for downstream user tenant.")]
    public string UserTenantId { get; set; } = default(string);

    [Option("userId", Required = true, SetName = "AllParameterSets", HelpText = "User Id for which token will be issued.")]
    public string UserId { get; set; } = default(string);

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        var authentication = serviceProvider.GetRequiredService<IMicrosoftAuthentication>();
        var gatewayConfig = serviceProvider.GetRequiredService<IOptionsMonitor<GatewayConfig>>();
        var neptuneDiscovery = serviceProvider.GetRequiredService<INeptuneDiscovery>();

        var cmd = new CommandEvaluateRoleAssignment(this, configuration, logger, gatewayConfig, neptuneDiscovery, authentication);
        var result = cmd.Run();
        return result;
    }
}