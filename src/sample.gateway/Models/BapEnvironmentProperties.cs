namespace sample.gateway.Models
{
    using Newtonsoft.Json;

    public class BapEnvironmentProperties
    {
        [JsonProperty(PropertyName = "createdTime")]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "provisioningState")]
        public string ProvisioningState { get; set; }

        [JsonProperty(PropertyName = "creationType")]
        public string CreationType { get; set; }

        [JsonProperty(PropertyName = "environmentSku")]
        public string EnvironmentSku { get; set; }

        [JsonProperty(PropertyName = "databaseType")]
        public string DatabaseType { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public BapPrincipal CreatedBy { get; set; }

        [JsonProperty(PropertyName = "usedBy")]
        public BapPrincipal UsedBy { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "scenarioName")]
        public string ScenarioName { get; set; }

    }
}
