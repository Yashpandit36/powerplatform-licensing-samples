namespace sample.gateway
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.Versioning;
    using System.Threading;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using sample.gateway.Discovery;

    public class CommandAllocationPut : BaseCommand<CommandAllocationPutOptions>
    {
        private readonly GatewayConfig _gatewayConfig;
        private readonly INeptuneDiscovery _neptuneDiscovery;
        private string _clientId;
        private Uri _authority;
        private IPublicClientApplication _clientApplication;

        public CommandAllocationPut(
            CommandAllocationPutOptions opts,
            IConfiguration configuration,
            IOptionsMonitor<GatewayConfig> gatewayConfig,
            INeptuneDiscovery neptuneDiscovery,
            ILogger logger) : base(opts, configuration, logger)
        {
            _gatewayConfig = gatewayConfig?.CurrentValue ?? throw new ArgumentNullException(nameof(gatewayConfig));
            _neptuneDiscovery = neptuneDiscovery ?? throw new ArgumentNullException(nameof(neptuneDiscovery));
        }

        public override void OnInit()
        {
            _clientId = PowershellClientId.ToString();

            _authority = new Uri(_gatewayConfig.AuthenticationEndpoint.GetScopeEnsureResourceTrailingSlash(Opts.TenantId));

            // Will will use a Public Client to obtain tokens interactively
            _clientApplication = PublicClientApplicationBuilder
              .Create(_clientId)
              .WithAuthority(_authority.ToString(), validateAuthority: true)
              .WithDefaultRedirectUri()
              .WithInstanceDiscovery(enableInstanceDiscovery: true)
              .Build();
        }

        // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
        [SupportedOSPlatform("windows")]
        public override int OnRun()
        {
            try
            {
                Guid correlationId = Guid.NewGuid(); // For tracing purposes, associate all calls in this run with this correlation ID

                // BAP Environment Discovery
                int pagingBy = 2; // Number of items to fetch per page
                Uri bapDomain = new Uri($"https://{_neptuneDiscovery.GetBapEndpoint()}"); // used to validate Nextlink contains relative base url
                Uri bapEnvironmentsUrl = new Uri(bapDomain, $"/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?api-version=2021-04-01&$top={pagingBy}&$select=name,properties.displayName,properties.createdTime,properties.tenantId,location");

                // Neptune PPAPI GW Tenant routing URL
                Uri gatewayTenantUri = new Uri($"https://{_neptuneDiscovery.GetTenantEndpoint(Opts.TenantId)}");
                Uri islandGatewayTenantUri = new Uri($"https://{_neptuneDiscovery.GetTenantIslandClusterEndpoint(Opts.TenantId)}");

                (bool flowControl, int value) = ProcessBapEnvironments(bapDomain, bapEnvironmentsUrl.ToString(), gatewayTenantUri, islandGatewayTenantUri, correlationId);
                if (!flowControl)
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.LogError(ex.Message);
                return -1;
            }
            return 0;
        }

        // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
        [SupportedOSPlatform("windows")]
        private (bool flowControl, int value) ProcessBapEnvironments(Uri bapBaseUri, string bapEnvironmentsUrl, Uri gatewayTenantUri, Uri islandGatewayTenantUri, Guid correlationId)
        {
            if (!IsSafeNextLink(bapBaseUri, bapEnvironmentsUrl))
            {
                TraceLogger.LogError("Unsafe nextLink detected, aborting operation.");
                return (flowControl: false, value: -1);
            }

            string gatewayResource = _neptuneDiscovery.GetTokenAudience();
            string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
            string tokenSuffix = "gateway";
            string gatewayAccessToken = OnAcquireUserToken(_clientApplication, _authority, _clientId, gatewayResource, tokenPrefix, tokenSuffix).GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(gatewayAccessToken))
            {
                TraceLogger.LogError("Failed to acquire token for gateway.");
                return (flowControl: false, value: -1);
            }

            string bapResource = _neptuneDiscovery.GetBapAudience();
            string bapAccessToken = OnAcquireUserToken(_clientApplication, _authority, _clientId, bapResource, tokenPrefix, "bap").GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(bapAccessToken))
            {
                TraceLogger.LogError("Failed to acquire token for BAP.");
                return (flowControl: false, value: -1);
            }

            string environmentsResponse = OnSendAsync(bapEnvironmentsUrl, Opts.TenantId, bapAccessToken, httpMethod: HttpMethod.Get, correlationId: correlationId, cancellationToken: CancellationToken.None);
            if (string.IsNullOrWhiteSpace(environmentsResponse))
            {
                TraceLogger.LogError("Failed to retrieve environments.");
                return (flowControl: false, value: -1);
            }

            BapPagedEntityResponse<BapEnvironmentScope> environments = JsonConvert.DeserializeObject<BapPagedEntityResponse<BapEnvironmentScope>>(environmentsResponse);
            foreach (BapEnvironmentScope environment in environments.Value)
            {
                TraceLogger.LogInformation("Environment: {EnvironmentName}, Display Name: {DisplayName}", environment.Name, environment.Properties.DisplayName);

                Uri allocationsUrl = new Uri(gatewayTenantUri, $"/licensing/allocations?$filter=environmentId eq '{environment.Name}' and EntitlementId in (MCSMessages,MCSSessions)&api-version=1");
                string allocationsResponse = OnSendAsync(allocationsUrl.ToString(), Opts.TenantId, gatewayAccessToken, HttpMethod.Get, correlationId: correlationId, cancellationToken: CancellationToken.None);
                if (string.IsNullOrWhiteSpace(allocationsResponse))
                {
                    TraceLogger.LogInformation("Failed to retrieve allocations for Environment: {EnvironmentName}.", environment.Name);
                }
                else
                {
                    // Can Put
                    AllocationResponseModel allocationResponse = JsonConvert.DeserializeObject<AllocationResponseModel>(allocationsResponse);

                    AllocationPutRequestModel allocationPutRequestModel = new AllocationPutRequestModel
                    {
                        Scope = allocationResponse.Scope,
                        AllocatedEntitlements = allocationResponse.AllocatedEntitlements ?? new List<EntitlementAllocationModel>(),
                    };

                    bool HasChanges = false;

                    // If no enforcement rules were found, defaults to Tenant Pool, lets create the entitlement
                    HasChanges = HasChanges || EnsureEntitlementEnforcement(allocationPutRequestModel, new EntitlementId("MCSMessages"), EnforcementRuleTypes.TenantPool);
                    HasChanges = HasChanges || EnsureEntitlementEnforcement(allocationPutRequestModel, new EntitlementId("MCSSessions"), EnforcementRuleTypes.TenantPool);

                    string assertedChange = JsonConvert.SerializeObject(allocationPutRequestModel, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new StringEnumConverter(),
                            new ModelTypeConverter<EntitlementId>(),
                            new ModelTypeConverter<EnvironmentGroupId>(),
                            new ModelTypeConverter<EnvironmentId>(),
                            new ModelTypeConverter<TenantId>(),
                            new ModelTypeConverter<UserId>(),
                        },
                    });

                    if (!HasChanges)
                    {
                        TraceLogger.LogInformation("No Enforcement Rules to unset for Environment: {EnvironmentName}", environment.Name);
                        continue; // No-Op
                    }

                    Uri allocationsPutUrl = new Uri(gatewayTenantUri, $"/licensing/allocations?api-version=1");

                    if (Opts.WhatIf.HasValue) // typically null, if this --whatif is present in the pipeline skip it
                    {
                        /// Present the change could be made
                        TraceLogger.LogInformation("WhatIf: Would PUT to {AllocationsPutUrl} with body: {AssertedChange}", allocationsPutUrl, assertedChange);
                    }
                    else
                    {
                        string putResponse = OnSendAsync(allocationsPutUrl.ToString(), Opts.TenantId, gatewayAccessToken, httpMethod: HttpMethod.Put, requestBody: assertedChange, correlationId: correlationId, cancellationToken: CancellationToken.None);
                        if (string.IsNullOrWhiteSpace(putResponse))
                        {
                            TraceLogger.LogError("Failed to PUT allocations for Environment: {EnvironmentName}", environment.Name);
                        }
                        else
                        {
                            TraceLogger.LogInformation("Successfully PUT allocations for Environment: {EnvironmentName}", environment.Name);
                            TraceLogger.LogInformation("Response: {PutResponse}", putResponse);
                        }
                    }
                }
            }

            // Check if there are more environments to process
            if (environments.HasMore())
            {
                // Handle pagination if needed
                TraceLogger.LogInformation("More environments to process, next link: {NextLink}", environments.NextLink);
                return ProcessBapEnvironments(bapBaseUri, environments.NextLink, gatewayTenantUri, islandGatewayTenantUri, correlationId);
            }

            return (flowControl: true, value: default);
        }

        private bool EnsureEntitlementEnforcement(AllocationPutRequestModel allocationPutRequestModel, EntitlementId entitlementId, EnforcementRuleTypes enforcementRuleType)
        {
            EntitlementAllocationModel existingEntitlement = allocationPutRequestModel.AllocatedEntitlements.FirstOrDefault(fn => fn.EntitlementId == entitlementId);
            if (existingEntitlement == null)
            {
                // Create the entitlement with default rules
                EntitlementAllocationModel newEntitlement = new EntitlementAllocationModel
                {
                    Allocation = new AllocationModel
                    {
                        Quantity = 0, // Default quantity, can be adjusted later
                        AutoAllocated = 0 // Default auto allocation, can be adjusted later
                    },
                    EntitlementId = entitlementId,
                    EnforcementRules = new List<EnforcementRule>
                {
                    new EnforcementRule
                    {
                        IsEnabled = Opts.Action == CommandAllocationPutOptionsAction.EnableDrawFromTenantPool,
                        Type = enforcementRuleType
                    }
                }
                };
                allocationPutRequestModel.AllocatedEntitlements.Add(newEntitlement);
                return true;
            }
            else
            {
                // Update the entitlement with default rules
                if (existingEntitlement.EnforcementRules == null || existingEntitlement.EnforcementRules.Count == 0)
                {
                    // backfill empty object with collection
                    existingEntitlement.EnforcementRules = new List<EnforcementRule>();
                }

                EnforcementRule entitlementRule = existingEntitlement.EnforcementRules.FirstOrDefault(er => er.Type == enforcementRuleType);
                if (entitlementRule == null)
                {
                    // If the entitlement exists but doesn't have the enforcement rule, add it
                    existingEntitlement.EnforcementRules.Add(new EnforcementRule
                    {
                        IsEnabled = Opts.Action == CommandAllocationPutOptionsAction.EnableDrawFromTenantPool,
                        Type = enforcementRuleType
                    });
                    return true;
                }
                else
                {
                    // If the entitlement exists and has the enforcement rule
                    if (entitlementRule.IsEnabled)
                    {
                        // ensure it's set to false
                        if (Opts.Action == CommandAllocationPutOptionsAction.DisableDrawFromTenantPool)
                        {
                            entitlementRule.IsEnabled = false;
                            return true; // Change was made
                        }
                    }
                    else
                    {
                        // ensure it's set to true
                        if (Opts.Action == CommandAllocationPutOptionsAction.EnableDrawFromTenantPool)
                        {
                            entitlementRule.IsEnabled = true;
                            return true; // Change was made
                        }
                    }
                }
            }

            return false;
        }

    }
}