namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // One entry per outstanding STRN for a given Buyer+Order — powers a "pick an STRN
    // from a list" dropdown as an alternate entry point to typing the STRN number
    // directly (see GetPendingStrnsByOrderAsync).
    public class GinPendingStrnServiceModel
    {
        public string StrnNumber { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
    }
}
