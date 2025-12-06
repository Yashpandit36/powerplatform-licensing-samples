namespace sample.gateway.Discovery
{

    using Microsoft.Extensions.Options;

    public class NeptuneDiscovery : INeptuneDiscovery
    {
        private static readonly string TenantInfix = "tenant";
        private static readonly string EnvironmentInfix = "environment";
        private const string TenantIslandPrefix = "il-";

        private readonly IOptionsMonitor<GatewayConfig> _gatewayConfig;
        private readonly IOptionsMonitor<PowerPlatformEndpointsSettings> _endpointSettings;
        private readonly string endpointSuffix;
        private readonly int idSuffixLength;

        public NeptuneDiscovery(
            IOptionsMonitor<GatewayConfig> gatewayConfig,
            IOptionsMonitor<PowerPlatformEndpointsSettings> endpointSettings)
        {
            _gatewayConfig = gatewayConfig;
            _endpointSettings = endpointSettings;
            this.endpointSuffix = GetEndpointSuffix(_gatewayConfig.CurrentValue.ClusterCategory);
            this.idSuffixLength = GetIdSuffixLength(_gatewayConfig.CurrentValue.ClusterCategory);
        }

        public ClusterCategory ClusterCategory => _gatewayConfig.CurrentValue.ClusterCategory;
        public ClusterType ClusterType => _gatewayConfig.CurrentValue.ClusterType;

        public string GetGatewayEndpoint(TenantId tenantId, EnvironmentId environmentId = default)
        {
            if (_gatewayConfig.CurrentValue.ClusterType == ClusterType.CustomerManagement)
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
            string categoryName = _gatewayConfig.CurrentValue.ClusterCategory.ToString();
            string configuredSuffix = _endpointSettings.CurrentValue.BapDnsZones?.TryGetValue(categoryName, out string suffix) == true ? suffix : null;

            if (!string.IsNullOrEmpty(configuredSuffix))
            {
                return configuredSuffix;
            }

            throw new ArgumentException($"Invalid cluster category value: {categoryName}", nameof(categoryName));
        }

        public string GetBapAudience()
        {
            string categoryName = _gatewayConfig.CurrentValue.ClusterCategory.ToString();
            string configuredSuffix = _endpointSettings.CurrentValue.BapDnsAudience?.TryGetValue(categoryName, out string suffix) == true ? suffix : null;

            if (!string.IsNullOrEmpty(configuredSuffix))
            {
                return $"https://{configuredSuffix}/";
            }

            throw new ArgumentException($"Invalid cluster category value: {categoryName}", nameof(categoryName));
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
            string categoryName = category.ToString();
            string configuredSuffix = _endpointSettings.CurrentValue.PowerPlatformApiEndpointSuffixes?.TryGetValue(categoryName, out string suffix) == true ? suffix : null;

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