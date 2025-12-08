namespace sample.gateway;
/// <summary>
/// Represents the options for the Billing Policy Get Environments check command
/// </summary>
/// <example>
/// CommandBillingPolicyEnvironmentGet --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0 --billingPolicyId 4b62a25e-1c3d-e2bc-9270-307db9f15b00
/// </example> 
[Verb("CommandBillingPolicyEnvironmentGet", HelpText = "This will use credentials from certificates to assert JWT tokens")]
public class CommandBillingPolicyEnvironmentGetOptions : CommandOptions
{
    [Option("billingPolicyId", Required = true, SetName = "AllParameterSets", HelpText = "Billing Policy Id for downstream user tenant.")]
    public string BillingPolicyId { get; set; } = default(string);

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandBillingPolicyEnvironmentGet cmd = new CommandBillingPolicyEnvironmentGet(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}