namespace sample.gateway.Models;

using Newtonsoft.Json;

public class AlertEnforcementRule : EnforcementRule
{
    [JsonProperty(PropertyName = "ruleData")]
    public AlertData RuleData { get; set; }
}