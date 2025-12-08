namespace sample.gateway;
/// <summary>
/// Represents the options for the Billing Policy Get List check command
/// </summary>
/// <example>
/// CommandBillingPoliciesGetOptions --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0
/// </example> 
[Verb("CommandBillingPoliciesGet", HelpText = "This will use credentials from certificates to assert JWT tokens")]
public class CommandBillingPoliciesGetOptions : CommandOptions
{
    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandBillingPoliciesGet cmd = new CommandBillingPoliciesGet(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}