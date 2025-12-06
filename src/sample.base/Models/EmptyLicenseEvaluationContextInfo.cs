namespace sample.gateway.Models;

using Newtonsoft.Json;

public class EmptyLicenseEvaluationContextInfo : LicenseEvaluationContextInfo
{
    public override string ConsumptionEventType => "None";

    public override string GetAppName()
    {
        return string.Empty;
    }

    public override AppType GetAppType()
    {
        return AppType.None;
    }

    public override NeptuneOperationType GetOperationType()
    {
        return NeptuneOperationType.Default;
    }

    public override AppPlanClassification GetAppPlanClassification()
    {
        return AppPlanClassification.None;
    }
}