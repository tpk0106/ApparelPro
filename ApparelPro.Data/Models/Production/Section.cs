namespace ApparelPro.Data.Models.Production
{
    // OD_SECT - production flow stages (Cutting, Sewing, Checking,
    // Finishing, Packing, Shipping in your live data). IsFinal marks the
    // section whose completion means the style is fully produced (Packing
    // in your data) - referenced by End of Production Confirmation later.
    public class Section
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }
}
