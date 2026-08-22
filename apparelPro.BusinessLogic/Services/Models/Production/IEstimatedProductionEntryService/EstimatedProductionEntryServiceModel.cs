namespace apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionEntryService
{
    public class EstimatedProductionEntryServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string LineCode { get; set; } = null!;
        public DateOnly Date { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
