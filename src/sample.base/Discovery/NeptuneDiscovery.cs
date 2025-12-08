namespace sample.gateway.Discovery;

using Microsoft.Extensions.Options;

public class NeptuneDiscovery : INeptuneDiscovery
{
    private static readonly string TenantInfix = "tenant";
    private static readonly string EnvironmentInfix = "environment";
    private const string TenantIslandPrefix = "il-";

    private readonly IOptionsMonitor<GatewayConfig> _gatewayConfig;
    private readonly IOptionsMonitor<PowerPlatformEndpointsSettings> _endpointSettings;

    public NeptuneDiscovery(
        IOptionsMonitor<GatewayConfig> gatewayConfig,
        IOptionsMonitor<PowerPlatformEndpointsSettings> endpointSettings)
    {
        _gatewayConfig = gatewayConfig;
        _endpointSettings = endpointSettings;
    }

    public ClusterCategory ClusterCategory => _gatewayConfig.CurrentValue.ClusterCategory;
    public ClusterType ClusterType => _gatewayConfig.CurrentValue.ClusterType;

    public string GetGatewayEndpoint(TenantId tenantId = default, EnvironmentId environmentId = default)
    {
        if (tenantId == default)
        {
            return GetGlobalEndpoint();
        }

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

    private string _endpointSuffix;
    private string EndpointSuffix
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_endpointSuffix))
            {
                _endpointSuffix = GetEndpointSuffix(_gatewayConfig.CurrentValue.ClusterCategory);
            }
            return _endpointSuffix;
        }
    }

    public string GetTokenAudience()
    {
        return "https://" + EndpointSuffix;
    }

    public string GetGlobalEndpoint()
    {
        return EndpointSuffix;
    }

    internal string GetTenantEndpoint(TenantId tenantId)
    {
        return BuildEndpoint(TenantInfix, tenantId.ToString());
    }

    internal string GetTenantIslandClusterEndpoint(TenantId tenantId)
    {
        return BuildEndpoint(TenantInfix, tenantId.ToString(), TenantIslandPrefix);
    }

    internal string GetEnvironmentEndpoint(EnvironmentId environmentId)
    {
        return BuildEndpoint(EnvironmentInfix, environmentId.ToString());
    }

    private int _idSuffixLength = 0;
    private int IdSuffixLength
    {
        get
        {
            if (_idSuffixLength == 0)
            {
                _idSuffixLength = GetIdSuffixLength(_gatewayConfig.CurrentValue.ClusterCategory);
            }
            return _idSuffixLength;
        }
    }
    private string BuildEndpoint(string infix, string resourceId, string prefix = "")
    {
        string text = resourceId.ToLower().Replace("-", "");
        string value = text.Substring(0, text.Length - IdSuffixLength);
        string value2 = text.Substring(text.Length - IdSuffixLength, IdSuffixLength);
        return $"{prefix}{value}.{value2}.{infix}.{EndpointSuffix}";
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