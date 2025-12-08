namespace sample.gateway;

[Verb("CommandSdkCapacityGet")]
public class CommandSdkCapacityGetOptions : CommandOptions
{
    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandSdkCapacityGet cmd = new CommandSdkCapacityGet(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}