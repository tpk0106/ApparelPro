namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GinPrintHeaderServiceModel
    {
        public string GinNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public string SourceStrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Same convention as StrnPrintHeaderServiceModel - legacy always prints the
        // current system date/time on every print run, not the original transaction date.
        public DateTime PrintedOn { get; set; }
    }
}
