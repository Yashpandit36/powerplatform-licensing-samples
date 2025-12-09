namespace sample.gateway;

[Verb("CommandAllocationEnvironmentsPatch")]
public class CommandAllocationEnvironmentsPatchOptions : CommandOptions
{
    [Option(shortName: 'a', "action", Required = false, SetName = "AllParameterSets", HelpText = "Action to perform")]
    public CommandAllocationEnvironmentsPatchOptionsAction Action { get; set; } = CommandAllocationEnvironmentsPatchOptionsAction.DisableDrawFromTenantPool;

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationEnvironmentsPatch cmd = new CommandAllocationEnvironmentsPatch(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}