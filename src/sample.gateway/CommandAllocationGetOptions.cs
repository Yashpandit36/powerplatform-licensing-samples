namespace sample.gateway;

[Verb("CommandAllocationGet")]
public class CommandAllocationGetOptions : CommandOptions
{
    [Option("environmentId", Required = false, SetName = "AllParameterSets", HelpText = "Environment Id for which token will be issued")]
    public string EnvironmentId { get; set; }

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationGet cmd = new CommandAllocationGet(this, configuration, logger, serviceProvider);
        int result = cmd.Run();
        return result;
    }
}