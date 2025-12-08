namespace sample.gateway.Discovery;

public interface INeptuneDiscovery
{
    ClusterCategory ClusterCategory { get; }
    ClusterType ClusterType { get; }

    string GetBapAudience();
    string GetBapEndpoint();

    /// <summary>
    /// Gets the gateway endpoint for the given geo and region.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="environmentId">The environment ID.</param>
    /// <returns></returns>
    string GetGatewayEndpoint(TenantId tenantId = default, EnvironmentId environmentId = default);
    string GetGlobalEndpoint();
    string GetTokenAudience();
}