namespace sample.gateway.Discovery;

public interface INeptuneDiscovery
{
    ClusterCategory ClusterCategory { get; }
    ClusterType ClusterType { get; }

    string GetBapAudience();
    string GetBapEndpoint();
    string GetEnvironmentEndpoint(EnvironmentId environmentId);

    /// <summary>
    /// Gets the gateway endpoint for the given geo and region.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="environmentId">The environment ID.</param>
    /// <returns></returns>
    string GetGatewayEndpoint(TenantId tenantId, EnvironmentId environmentId = default);
    string GetGlobalEndpoint();
    string GetTenantEndpoint(TenantId tenantId);
    string GetTenantIslandClusterEndpoint(TenantId tenantId);
    string GetTokenAudience();
}