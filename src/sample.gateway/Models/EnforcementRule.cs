namespace sample.gateway.Models
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(EnforcementRuleConverter))]
    public class EnforcementRule
    {
        [JsonProperty(PropertyName = "ruleType")]
        public EnforcementRuleTypes Type { get; set; }
        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled { get; set; }
    }
}