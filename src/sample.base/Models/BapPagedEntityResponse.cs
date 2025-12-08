namespace sample.gateway.Models;

using Newtonsoft.Json;

using System.Collections.Generic;

public class BapPagedEntityResponse<T>
{
    [JsonProperty(PropertyName = "value")]
    public IEnumerable<T> Value { get; set; }

    [JsonProperty(PropertyName = "nextLink")]
    public string NextLink { get; set; }

    public bool HasMore()
    {
        return !string.IsNullOrWhiteSpace(NextLink);
    }
}