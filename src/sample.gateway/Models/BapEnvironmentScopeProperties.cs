namespace sample.gateway.Models
{
    using Newtonsoft.Json;

    public class BapEnvironmentScopeProperties
    {
        [JsonProperty(PropertyName = "createdTime")]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "azureRegionHint")]
        public string AzureRegionHint { get; set; }

        [JsonProperty(PropertyName = "provisioningState")]
        public string ProvisioningState { get; set; }

        [JsonProperty(PropertyName = "databaseType")]
        public string DatabaseType { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public BapPrincipal CreatedBy { get; set; }

    }
}