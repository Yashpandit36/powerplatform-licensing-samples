namespace sample.gateway;

[Verb("CommandCapacityGet")]
public class CommandCapacityGetOptions : CommandOptions
{
    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandCapacityGet cmd = new CommandCapacityGet(this, configuration, logger, serviceProvider);
        int result = cmd.Run();
        return result;
    }
}