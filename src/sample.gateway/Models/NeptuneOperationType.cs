namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    public enum NeptuneOperationType
    {
        Default = 0,
        Execute,
        View,
        Logins,
        AppLogins,
        Train,
        Predict,
        BusinessCardPredict,
        Conversations,
        PowerPagesAuthenticated,
        PowerPagesAnonymous,
        PortalAddOns,
        Save,
        MCSMessages, // todo: update per spec
        SCMessages, // todo: update per spec
        MCSSessions // todo: update per spec
    }
}