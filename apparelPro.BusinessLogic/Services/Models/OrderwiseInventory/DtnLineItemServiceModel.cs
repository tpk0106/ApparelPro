namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DtnLineItemServiceModel
    {
        // The item under the From Buyer/Order's stock the quantity is drawn from.
        public string FromItemCode { get; set; } = null!;
        // The item under the To Buyer/Order's stock the quantity becomes - the one place
        // DTN differs from GTN, which requires the same item code on both sides.
        public string ToItemCode { get; set; } = null!;
        // Basis/Store - shared by both sides, same convention as legacy IN_DTN1.PRG's
        // single m_store_cd field reused for both the From and To stock lookups.
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
