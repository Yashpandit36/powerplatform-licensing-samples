namespace sample.gateway.Discovery
{

    using Microsoft.Extensions.Options;

    public class NeptuneDiscovery : INeptuneDiscovery
    {
        private static readonly string TenantInfix = "tenant";
        private static readonly string EnvironmentInfix = "environment";
        private const string TenantIslandPrefix = "il-";

        private readonly ClusterCategory _clusterCategory;
        private readonly ClusterType _clusterType;
        private readonly GatewayConfig _gatewayConfig;
        private readonly PowerPlatformEndpointsSettings _endpointSettings;
        private readonly string endpointSuffix;
        private readonly int idSuffixLength;

        public NeptuneDiscovery(
            IOptionsMonitor<GatewayConfig> gatewayConfig,
            IOptionsMonitor<PowerPlatformEndpointsSettings> endpointSettings)
        {
            _gatewayConfig = gatewayConfig.CurrentValue;
            _endpointSettings = endpointSettings.CurrentValue;
            _clusterCategory = _gatewayConfig.ClusterCategory;
            _clusterType = _gatewayConfig.ClusterType;
            this.endpointSuffix = GetEndpointSuffix(_clusterCategory);
            this.idSuffixLength = GetIdSuffixLength(_clusterCategory);
        }

        public ClusterCategory ClusterCategory => _clusterCategory;
        public ClusterType ClusterType => _clusterType;

        public string GetGatewayEndpoint(TenantId tenantId, EnvironmentId environmentId = default)
        {
            if (_clusterType == ClusterType.CustomerManagement)
            {
                if (environmentId != default)
                {
                    return GetEnvironmentEndpoint(environmentId);
                }
                return GetTenantEndpoint(tenantId);
            }
            else
            {
                if (environmentId != default)
                {
                    return GetEnvironmentEndpoint(environmentId);
                }

                return GetTenantIslandClusterEndpoint(tenantId);
            }
        }

        public string GetBapEndpoint()
        {
            var categoryName = _clusterCategory.ToString();
            var configuredSuffix = _endpointSettings.BapDnsZones?.TryGetValue(categoryName, out var suffix) == true ? suffix : null;

            if (!string.IsNullOrEmpty(configuredSuffix))
            {
                return configuredSuffix;
            }

            throw new ArgumentException($"Invalid cluster category value: {categoryName}", nameof(categoryName));
        }

        public string GetBapAudience()
        {
            return $"https://{GetBapEndpoint()}/";
        }

        public string GetTokenAudience()
        {
            return "https://" + endpointSuffix;
        }

        public string GetGlobalEndpoint()
        {
            return endpointSuffix;
        }

        public string GetTenantEndpoint(TenantId tenantId)
        {
            return BuildEndpoint(TenantInfix, tenantId.ToString());
        }

        public string GetTenantIslandClusterEndpoint(TenantId tenantId)
        {
            return BuildEndpoint(TenantInfix, tenantId.ToString(), TenantIslandPrefix);
        }

        public string GetEnvironmentEndpoint(EnvironmentId environmentId)
        {
            return BuildEndpoint(EnvironmentInfix, environmentId.ToString());
        }

        private string BuildEndpoint(string infix, string resourceId, string prefix = "")
        {
            string text = resourceId.ToLower().Replace("-", "");
            string value = text.Substring(0, text.Length - idSuffixLength);
            string value2 = text.Substring(text.Length - idSuffixLength, idSuffixLength);
            return $"{prefix}{value}.{value2}.{infix}.{endpointSuffix}";
        }

        private string GetEndpointSuffix(ClusterCategory category)
        {
            var categoryName = category.ToString();
            var configuredSuffix = _endpointSettings.PowerPlatformApiEndpointSuffixes?.TryGetValue(categoryName, out var suffix) == true ? suffix : null;

            if (!string.IsNullOrEmpty(configuredSuffix))
            {
                return configuredSuffix;
            }

            throw new ArgumentException($"Invalid cluster category value: {category}", nameof(category));
        }

        private int GetIdSuffixLength(ClusterCategory category)
        {
            switch (category)
            {
                case ClusterCategory.FirstRelease:
                case ClusterCategory.Prod:
                    return 2;
                default:
                    return 1;
            }
        }
    }

}