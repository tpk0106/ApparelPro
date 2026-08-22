namespace apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionEntryService
{
    public class CreateDailyProductionEntryServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public decimal Hours { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
