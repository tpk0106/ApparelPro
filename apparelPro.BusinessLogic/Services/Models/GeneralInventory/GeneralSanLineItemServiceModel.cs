namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSanLineItemServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;

        // Absolute new stock count for this item - a physical stock-take correction, not
        // an add/subtract adjustment. Mirrors legacy's "repl qty_in_hd with m_qty".
        public decimal Quantity { get; set; }

        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = null!;
    }
}
