namespace sample.gateway.Models;

using Newtonsoft.Json;

public class BapEnvironment
{
    public BapEnvironment()
    {
        Properties = new BapEnvironmentProperties();
    }

    [JsonProperty(PropertyName = "location")]
    public string Location { get; set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "properties")]
    public BapEnvironmentProperties Properties { get; set; }
}
