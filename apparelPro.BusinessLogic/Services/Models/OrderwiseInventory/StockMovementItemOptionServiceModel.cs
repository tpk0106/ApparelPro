namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Item picker option for the "Stock Movement (for an Item)" report's Buyer -> Order ->
    // Item cascade. Sourced from StyleMaterialCostProfiles for the selected Buyer/Order -
    // mirrors legacy IN_SMVE1.PRG's item lookup, which is built from a temp file copied
    // out of od_sacc2 filtered to the same Buyer/Order (the "prch_tmp" file in that source).
    public class StockMovementItemOptionServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
