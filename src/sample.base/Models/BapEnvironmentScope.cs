namespace sample.gateway.Models;

using Newtonsoft.Json;

public class BapEnvironmentScope
{
    public BapEnvironmentScope()
    {
        Properties = new BapEnvironmentScopeProperties();
    }

    [JsonProperty(PropertyName = "location")]
    public string Location { get; set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "properties")]
    public BapEnvironmentScopeProperties Properties { get; set; }
}