namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class SectionProgressServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = string.Empty;
        public decimal ToDateQuantity { get; set; }

        // The contract section's own ToDateQuantity, repeated on every row -
        // lets the frontend draw each bar as a percentage of the ceiling
        // without a second round trip.
        public decimal CeilingQuantity { get; set; }
    }
}
