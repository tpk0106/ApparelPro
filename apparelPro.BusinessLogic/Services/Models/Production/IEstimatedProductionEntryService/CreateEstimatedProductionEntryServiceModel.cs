namespace apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionEntryService
{
    public class CreateEstimatedProductionEntryServiceModel
    {
        public DateOnly Date { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
