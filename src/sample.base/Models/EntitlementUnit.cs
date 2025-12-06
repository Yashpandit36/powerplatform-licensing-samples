namespace sample.gateway.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum EntitlementUnit
{
    NotSpecified = 0,
    MB = 1,
    Count = 2,
}