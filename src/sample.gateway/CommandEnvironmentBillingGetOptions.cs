namespace sample.gateway;
/// <summary>
/// Represents the options for the Entitlement check command, which uses the gateway cluster or island tenant/environment cluster routes
/// </summary>
/// <example>
/// 
/// Check Environment Entitlement using the Environment Route
/// CommandEnvironmentBillingGet --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0 --environmentId 4b62a25e-1c3d-e2bc-9270-307db9f15b00
/// 
/// </example> 
[Verb("CommandEnvironmentBillingGet", HelpText = "This will use credentials from certificates to assert JWT tokens")]
public class CommandEnvironmentBillingGetOptions : CommandOptions
{
    [Option("environmentId", Required = true, SetName = "AllParameterSets", HelpText = "Environment Id for downstream user tenant.")]
    public string EnvironmentId { get; set; } = default(string);

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandEnvironmentBillingGet cmd = new CommandEnvironmentBillingGet(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}