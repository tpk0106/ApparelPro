namespace ApparelPro.Data.Models.Production
{
    // PR_ESTD - planned daily output for a style already allocated to a
    // production line (PR_ESTD1.PRG). One row per Buyer/Order/Type/Style/
    // Line/Date.
    public class EstimatedProductionEntry
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
