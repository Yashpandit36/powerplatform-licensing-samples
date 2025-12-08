namespace sample.gateway.Models;

using Newtonsoft.Json;

public class AlertData
{
    [JsonProperty(PropertyName = "unitType")]
    public string UnitType { get; set; }
    [JsonProperty(PropertyName = "value")]
    public string Value { get; set; }
}