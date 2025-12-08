namespace sample.gateway.Tests;

using System;
using Microsoft.Extensions.Options;
using Moq;
using sample.gateway.Discovery;
using Xunit;

public class NeptuneDiscoveryTests
{
    private readonly Mock<IOptionsMonitor<GatewayConfig>> mockOptions = new();
    private readonly Mock<IOptionsMonitor<PowerPlatformEndpointsSettings>> mockEndpointOptions = new();
    private readonly NeptuneDiscovery neptuneDiscovery;

    public NeptuneDiscoveryTests()
    {
        mockEndpointOptions.Setup(m => m.CurrentValue)
            .Returns(new PowerPlatformEndpointsSettings
            {
                BapDnsZones = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Dev", "bap.dev.endpoint.com" },
                    { "Test", "bap.test.endpoint.com" },
                    { "Prod", "bap.prod.endpoint.com" },
                    { "Preprod", "bap.canary.endpoint.com" },
                    { "FirstRelease", "bap.canary.endpoint.com" },
                    { "GovFR", "bap.canary.endpoint.com" },
                    { "Gov", "bap.canary.endpoint.com" },
                    { "High", "bap.canary.endpoint.com" },
                    { "DoD", "bap.canary.endpoint.com" },
                    { "Mooncake", "bap.canary.endpoint.com" },
                    { "Ex", "bap.canary.endpoint.com" },
                    { "Rx", "bap.canary.endpoint.com" },
                },
                BapDnsAudience = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Dev", "bap.dev.endpoint.com" },
                    { "Test", "bap.test.endpoint.com" },
                    { "Prod", "bap.prod.endpoint.com" },
                    { "Preprod", "bap.canary.endpoint.com" },
                    { "FirstRelease", "bap.canary.endpoint.com" },
                    { "GovFR", "bap.canary.endpoint.com" },
                    { "Gov", "bap.canary.endpoint.com" },
                    { "High", "bap.canary.endpoint.com" },
                    { "DoD", "bap.canary.endpoint.com" },
                    { "Mooncake", "bap.canary.endpoint.com" },
                    { "Ex", "bap.canary.endpoint.com" },
                    { "Rx", "bap.canary.endpoint.com" },
                },
                PowerPlatformApiEndpointSuffixes = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Dev", "gateway.test.powerplatform.com" },
                    { "Test", "gateway.test.powerplatform.com" },
                    { "Prod", "gateway.prod.powerplatform.com" },
                    { "Preprod", "gateway.canary.powerplatform.com" },
                    { "FirstRelease", "gateway.canary.powerplatform.com" },
                    { "GovFR", "gateway.canary.powerplatform.com" },
                    { "Gov", "gateway.canary.powerplatform.com" },
                    { "High", "gateway.canary.powerplatform.com" },
                    { "DoD", "gateway.canary.powerplatform.com" },
                    { "Mooncake", "gateway.canary.powerplatform.com" },
                    { "Ex", "gateway.canary.powerplatform.com" },
                    { "Rx", "gateway.canary.powerplatform.com" },
                },
            });

        neptuneDiscovery = new NeptuneDiscovery(mockOptions.Object, mockEndpointOptions.Object);
    }

    [Fact]
    public void Cstr_MgmtPlane_Ok()
    {
        TenantId tenantId = (TenantId)Guid.Parse("03ab3068-c403-406d-8351-bdbb6374c8b0");
        EnvironmentId environmentId = (EnvironmentId)Guid.Parse("f67cbc8b-45af-e098-b776-441459089344").ToString();

        mockOptions.Setup(m => m.CurrentValue)
            .Returns(new GatewayConfig
            {
                ClusterCategory = ClusterCategory.Dev,
                ClusterType = ClusterType.CustomerManagement,
            });

        Assert.NotNull(neptuneDiscovery);

        string result = neptuneDiscovery.GetGatewayEndpoint(tenantId);
        Assert.Equal("03ab3068c403406d8351bdbb6374c8b.0.tenant.gateway.test.powerplatform.com", result);

        result = neptuneDiscovery.GetGatewayEndpoint(tenantId, environmentId);
        Assert.Equal("f67cbc8b45afe098b77644145908934.4.environment.gateway.test.powerplatform.com", result);

        result = neptuneDiscovery.GetBapEndpoint();
        Assert.Equal("bap.dev.endpoint.com", result);
        result = neptuneDiscovery.GetBapAudience();
        Assert.Equal("https://bap.dev.endpoint.com/", result);
        Assert.Equal(ClusterCategory.Dev, neptuneDiscovery.ClusterCategory);
        Assert.Equal(ClusterType.CustomerManagement, neptuneDiscovery.ClusterType);
    }

    [Fact]
    public void Cstr_MgmtPlane_Discovery_ClusterCategory()
    {
        ClusterCategory[] settings = Enum.GetValues<ClusterCategory>();

        foreach (ClusterCategory setting in settings)
        {
            mockOptions.Setup(m => m.CurrentValue)
                .Returns(new GatewayConfig
                {
                    ClusterCategory = setting,
                    ClusterType = ClusterType.CustomerManagement,
                });

            NeptuneDiscovery catNeptuneDiscovery = new NeptuneDiscovery(mockOptions.Object, mockEndpointOptions.Object);
            Assert.NotNull(catNeptuneDiscovery);
        }
    }

    [Fact]
    public void Cstr_DataPlane_Ok()
    {
        TenantId tenantId = (TenantId)Guid.Parse("03ab3068-c403-406d-8351-bdbb6374c8b0");
        EnvironmentId environmentId = (EnvironmentId)Guid.Parse("f67cbc8b-45af-e098-b776-441459089344").ToString();

        mockOptions.Setup(m => m.CurrentValue)
            .Returns(new GatewayConfig
            {
                ClusterCategory = ClusterCategory.Test,
                ClusterType = ClusterType.IslandCluster,
            });

        Assert.NotNull(neptuneDiscovery);

        string result = neptuneDiscovery.GetGatewayEndpoint(tenantId);
        Assert.Equal("il-03ab3068c403406d8351bdbb6374c8b.0.tenant.gateway.test.powerplatform.com", result);

        result = neptuneDiscovery.GetGatewayEndpoint(tenantId, environmentId);
        Assert.Equal("f67cbc8b45afe098b77644145908934.4.environment.gateway.test.powerplatform.com", result);
    }

    [Fact]
    public void Cstr_DataPlane_Discovery_ClusterCategory()
    {
        ClusterCategory[] settings = Enum.GetValues<ClusterCategory>();

        foreach (ClusterCategory setting in settings)
        {
            mockOptions.Setup(m => m.CurrentValue)
                .Returns(new GatewayConfig
                {
                    ClusterCategory = setting,
                    ClusterType = ClusterType.IslandCluster,
                });

            Assert.NotNull(neptuneDiscovery);
        }
    }
}