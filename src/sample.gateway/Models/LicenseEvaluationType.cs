namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum LicenseEvaluationType
    {
        NotSpecified = 0,
        Usl = 1,
        AutoClaim = 2,
        Capacity = 3,
        PayGoOnlyCapacity = 4,
        CapacityAndResourceLimit = 5

    }
}