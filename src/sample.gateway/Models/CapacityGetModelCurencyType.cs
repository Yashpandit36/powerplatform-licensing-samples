namespace sample.gateway.Models
{
    using System.Runtime.Serialization;

    public enum CapacityGetModelCurencyType
    {
        [EnumMember(Value = "AI")]
        AI,
        [EnumMember(Value = "AppPass")]
        AppPass,
        [EnumMember(Value = "AppPassForTeams")]
        AppPassForTeams,
        [EnumMember(Value = "Invoice")]
        Invoice,
        [EnumMember(Value = "MCSSessions")]
        MCSSessions,
        [EnumMember(Value = "MCSMessages")]
        MCSMessages,
        [EnumMember(Value = "PAHostedRPA")]
        PAHostedRPA,
        [EnumMember(Value = "PAUnattendedRPA")]
        PAUnattendedRPA,
        [EnumMember(Value = "PerFlowPlan")]
        PerFlowPlan,
        [EnumMember(Value = "PortalAddOns")]
        PortalAddOns,
        [EnumMember(Value = "PortalLogins")]
        PortalLogins,
        [EnumMember(Value = "PortalViews")]
        PortalViews,
        [EnumMember(Value = "PowerPagesAuthenticated")]
        PowerPagesAuthenticated,
        [EnumMember(Value = "PowerPagesAnonymous")]
        PowerPagesAnonymous,
        [EnumMember(Value = "PowerAutomatePerProcess")]
        PowerAutomatePerProcess,
        [EnumMember(Value = "ProcessMiningDataStorage")]
        ProcessMiningDataStorage,
        [EnumMember(Value = "SCMessages")]
        SCMessages,
        [EnumMember(Value = "VAConversations")]
        VAConversations,
        [EnumMember(Value = "W365APAYGO")]
        W365APAYGO,
    }
}
