namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class SupplierPoStageDetailAPIModel
    {
        public decimal RaisedQuantity { get; set; }
        public decimal RaisedValue { get; set; }
        public decimal OutstandingQuantity { get; set; }
        public decimal OutstandingValue { get; set; }
        public string Currency { get; set; } = "";
    }
}
