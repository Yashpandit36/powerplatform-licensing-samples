namespace sample.gateway.Models;

public class CapacityGetModel
{
    public long? Allocated { get; set; }

    public CapacityGetModelCurrencyConsumption Consumed { get; set; }

    public CapacityGetModelCurencyType? CurrencyType { get; set; }

    public long? Purchased { get; set; }
}
