
namespace ApparelPro.Data.Models.OrderManagement
{
    public class ColorSizeDetails
    {
        // Composite Primary Key matches your legacy SQL Server table setup
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public int TypeCode { get; set; }
        public string StyleCode { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal Ratio { get; set; }
        public decimal Qty { get; set; }
        // FIXED (2026-08-07): the Colour Target Allocation Setup screen lets the
        // user enter a free-text description/shade name per colour, but this
        // table previously had nowhere to persist it - the frontend silently
        // dropped it on save, and re-hydrated a fabricated placeholder string
        // on reload. Denormalized per-row (same as Color already is), matching
        // this table's existing flat/legacy-style design.
        public string? Description { get; set; }
    }
}
