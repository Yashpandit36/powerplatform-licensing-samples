namespace sample.gateway.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum ProductCategory
{
    NotSpecified = 0,
    D365Apps = 1,
    Dataverse = 2,
    Fno = 3,
    PowerApps = 4,
    PowerAutomate = 5,
    PowerPages = 6,
    PowerVirtualAgent = 7,
    CopilotStudio = 8,
    PowerPlatform = 9,
    Project = 10,
    W365 = 11
}