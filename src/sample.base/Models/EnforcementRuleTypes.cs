namespace sample.gateway.Models;

using Newtonsoft.Json;

public enum EnforcementRuleTypes
{
    NotSpecified = 0,
    Alert = 1,
    PayGo = 2,
    TenantPool = 3,
    Deny = 4
}