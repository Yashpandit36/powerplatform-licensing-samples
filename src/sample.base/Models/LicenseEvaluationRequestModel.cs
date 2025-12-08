namespace sample.gateway.Models;

using Newtonsoft.Json;
using System.Collections.Generic;

public class LicenseEvaluationRequestModel<T> where T : LicenseEvaluationContextInfo, new()
{
    public List<LicenseEvaluationType> EvaluationTypes { get; set; }
    public ProductCategory ProductCategory { get; set; }
    public HashSet<string> Entitlements { get; set; }
    public T ContextInformation { get; set; }
}