namespace sample.gateway.Discovery
{
    public sealed class GatewayConfig
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SectionName = "Gateway";

        public string AuthenticationEndpoint { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public ClusterCategory ClusterCategory { get; set; }
        public ClusterType ClusterType { get; set; }
        public string Environment { get; set; }
        public string FirstPartyInfrastructureTenantId { get; set; }
        public string ServiceAuthRegionalCertSubjectName { get; set; }

    }
}