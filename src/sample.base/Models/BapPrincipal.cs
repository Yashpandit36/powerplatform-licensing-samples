namespace sample.gateway.Models;

using Newtonsoft.Json;

public class BapPrincipal
{
    /// <summary>
    /// Gets or sets the principal id.
    /// possible values: "System", Guids
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the principal object type.
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }
}