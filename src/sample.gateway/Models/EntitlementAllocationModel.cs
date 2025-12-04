namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;

    public class EntitlementAllocationModel
    {
        public AllocationModel Allocation { get; set; }
        public EntitlementId EntitlementId { get; set; }
        public List<EnforcementRule> EnforcementRules { get; set; }

        public bool IsEntitlementAllocated()
        {
            if (Allocation != null
                && (Allocation.Quantity > 0 || Allocation.AutoAllocated > 0))
            {
                return true;
            }
            if (EnforcementRules != null)
            {
                return EnforcementRules.Any();
            }
            return false;
        }
    }
}