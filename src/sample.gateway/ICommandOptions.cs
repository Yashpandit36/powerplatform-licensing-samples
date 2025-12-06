namespace sample.gateway;

public interface ICommandOptions
{
    bool? WhatIf { get; set; }

    int NumberOfAttempts { get; set; }
}