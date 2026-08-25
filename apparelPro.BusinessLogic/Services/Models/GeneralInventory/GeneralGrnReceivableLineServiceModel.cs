namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGrnReceivableLineServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal Balance { get; set; } // context only - not enforced on commit, see class remarks
        public decimal QtyInHand { get; set; }
        public decimal MaxStock { get; set; }
    }
}
