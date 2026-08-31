namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ArnLineItemServiceModel
    {
        // Unlike AIN (one Buyer/Order/Process for the whole note), legacy IN_ARN4.PRG
        // lets every line carry its own Buyer/Order/Process - a single ARN can receive
        // processed goods back for several different orders in one note.
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string AdditionalProcessCode { get; set; } = null!;

        public string ItemCode { get; set; } = null!; // 22-char composite key
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        // StoreCode is NOT client-supplied - legacy reads it off the matched
        // GarmentAdditionalCost row (od_aitm->store_cd), resolved server-side on commit.
    }
}
