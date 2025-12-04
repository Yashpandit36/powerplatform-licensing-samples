namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public record LicensedRoleAssignmentsRequestModel
    {
        /// <summary>
        /// Gets or sets the list of role ids to evaluate.
        /// </summary>
        public List<string> RoleIds { get; init; }
    }
}