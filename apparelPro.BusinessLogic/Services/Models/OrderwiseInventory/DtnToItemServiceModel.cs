namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // One candidate destination item for a Direct Goods Transfer Note line - sourced from
    // the To Buyer/Order's own material requirement (StyleMaterialCostProfiles), mirroring
    // legacy IN_DTN1.PRG's "seek od_sacc2 for xtbuyer+xtorder" browse.
    public class DtnToItemServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
    }
}
