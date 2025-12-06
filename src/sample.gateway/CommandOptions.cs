namespace sample.gateway
{
    /// <summary>
    /// Common options for all commands
    /// </summary>
    public class CommandOptions : ICommandOptions
    {
        [Option('w', "whatif", Required = false, HelpText = "provides for what if scenarios, present changes before asserting them.")]
        public bool? WhatIf { get; set; }

        [Option("num-attempts", Default = 1, Required = false, SetName = "AllParameterSets", HelpText = "Number of attempts to retry the request.")]
        public int NumberOfAttempts { get; set; }
    }
}