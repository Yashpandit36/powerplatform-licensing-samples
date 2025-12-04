namespace sample.gateway.Discovery
{
    using System.Runtime.Serialization;

    public enum ClusterType
    {
        [EnumMember(Value = "IslandCluster")]
        IslandCluster,
        [EnumMember(Value = "CustomerManagement")]
        CustomerManagement
    }
}
