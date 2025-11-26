namespace sample.gateway.Models
{
    using Newtonsoft.Json;

    public class AllocationModel
    {
        public double Quantity { get; set; }
        public double AutoAllocated { get; set; }
        public EntitlementUnit Unit { get; set; }
    }
}