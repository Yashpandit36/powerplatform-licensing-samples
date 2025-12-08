namespace sample.gateway.Models;

using Newtonsoft.Json;

public abstract class LicenseEvaluationContextInfo
{
    public ProductCategory ProductCategory { get; set; }
    public abstract string ConsumptionEventType { get; }
    public abstract AppType GetAppType();
    public abstract NeptuneOperationType GetOperationType();
    public abstract string GetAppName();
    public abstract AppPlanClassification GetAppPlanClassification();
}