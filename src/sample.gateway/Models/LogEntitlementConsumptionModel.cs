namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class LogEntitlementConsumptionModel
    {
        /// <summary>
        /// Gets the ID of the entitlement that is consumed.
        /// </summary>
        public string EntitlementId { get; set; }

        /// <summary>
        /// Gets the unique identifier for the consumption.
        /// </summary>
        public string ConsumptionId { get; set; }

        /// <summary>
        /// Gets the unique identifier for the resource.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets the amount of the entitlement consumed.
        /// </summary>
        public double ConsumedQuantity { get; set; }

        /// <summary>
        /// Gets a value indicating whether the consumption is billable, if false it indicates these entries are either used by trial or prepaid USL.
        /// </summary>
        public bool IsBillable { get; set; }

        /// <summary>
        /// Gets a bag of additional per-RP properties.
        /// </summary>
        public Dictionary<string, object> Tags { get; set; }
    }
}