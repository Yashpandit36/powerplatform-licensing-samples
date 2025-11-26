namespace sample.gateway.Models
{
    using Newtonsoft.Json;

    public enum AppType
    {
        None = 0,
        CanvasApp = 1,
        ModelApp = 2,
        PowerAppsPortal = 3,
        Flow = 4,
        AIBuilder = 5,
        Api = 6,
        VirtualAgents = 7,
        TeamsApp = 8,
        Workflow = 9,
        CloudFlow = 10,
        DesktopFlowAttended = 11,
        UnifiedApp = 12
    }
}