namespace sample.gateway.Models;

using Newtonsoft.Json;

[ExcludeFromCodeCoverage]
public class ScopeModel
{
    [JsonProperty(PropertyName = "tenantId")]
    public TenantId TenantId { get; set; }

    [JsonProperty(PropertyName = "environmentGroupId")]
    public EnvironmentGroupId? EnvironmentGroupId { get; set; }

    [JsonProperty(PropertyName = "environmentId")]
    public EnvironmentId? EnvironmentId { get; set; }

    [JsonProperty(PropertyName = "resourceId")]
    public string ResourceId { get; set; }

    [JsonProperty(PropertyName = "userId")]
    public UserId? UserId { get; set; }
}