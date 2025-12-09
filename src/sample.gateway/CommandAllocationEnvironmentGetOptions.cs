namespace sample.gateway;

[Verb("CommandAllocationEnvironmentGet")]
public class CommandAllocationEnvironmentGetOptions : CommandOptions
{
    [Option("environmentId", Required = true, SetName = "AllParameterSets", HelpText = "Environment Id for which token will be issued")]
    public string EnvironmentId { get; set; }

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationEnvironmentGet cmd = new CommandAllocationEnvironmentGet(this, configuration, logger, serviceProvider);
        int result = cmd.Run();
        return result;
    }
}