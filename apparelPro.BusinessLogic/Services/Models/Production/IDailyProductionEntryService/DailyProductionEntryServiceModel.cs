namespace apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionEntryService
{
    public class DailyProductionEntryServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }

        // Computed on read (SUM of Quantity where Date <= this entry's
        // date, for this section) - never stored, replacing legacy's
        // manually-cascaded to_dt_qty / PR_MPROD.
        public decimal ToDateQuantity { get; set; }
    }
}
