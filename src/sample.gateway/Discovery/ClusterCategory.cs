namespace sample.gateway.Discovery
{
    using System.Runtime.Serialization;

    public enum ClusterCategory
    {
        [EnumMember(Value = "Dev")]
        Dev,
        [EnumMember(Value = "Test")]
        Test,
        [EnumMember(Value = "Preprod")]
        Preprod,
        [EnumMember(Value = "FirstRelease")]
        FirstRelease,
        [EnumMember(Value = "Prod")]
        Prod,
        [EnumMember(Value = "Gov")]
        Gov,
        [EnumMember(Value = "High")]
        High,
        [EnumMember(Value = "DoD")]
        DoD,
        [EnumMember(Value = "Mooncake")]
        Mooncake,
        [EnumMember(Value = "Ex")]
        Ex,
        [EnumMember(Value = "Rx")]
        Rx,
        [EnumMember(Value = "Local")]
        Local,
        [EnumMember(Value = "GovFR")]
        GovFR
    }
}
