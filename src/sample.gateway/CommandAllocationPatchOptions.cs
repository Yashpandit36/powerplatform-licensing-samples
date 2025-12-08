namespace sample.gateway;

[Verb("CommandAllocationPatch")]
public class CommandAllocationPatchOptions : CommandOptions
{
    [Option(shortName: 'a', "action", Required = false, SetName = "AllParameterSets", HelpText = "Action to perform")]
    public CommandAllocationPatchOptionsAction Action { get; set; } = CommandAllocationPatchOptionsAction.DisableDrawFromTenantPool;

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationPatch cmd = new CommandAllocationPatch(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}