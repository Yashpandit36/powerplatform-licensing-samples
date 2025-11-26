namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class AllocationResponseModel
    {

        /// <summary>
        /// Scope of the allocation.
        /// </summary>
        public ScopeModel Scope { get; set; }

        /// <summary>
        /// Allocated entitlements.
        /// </summary>
        public List<EntitlementAllocationModel> AllocatedEntitlements { get; set; }
    }
}