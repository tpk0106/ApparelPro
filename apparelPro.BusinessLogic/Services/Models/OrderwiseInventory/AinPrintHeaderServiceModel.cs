namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class AinPrintHeaderServiceModel
    {
        public string AinNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public string SubContractorCode { get; set; } = null!;
        public string AdditionalProcessCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Legacy IN_AIN2.PRG prints the current system date/time on every print run,
        // not the original transaction date - the note can be reprinted on demand.
        public DateTime PrintedOn { get; set; }
    }
}
