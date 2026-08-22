namespace apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService
{
    public class StyleOperationBreakdownServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public int ComponentSequence { get; set; }
        public int OperationNumber { get; set; }
        public string ComponentCode { get; set; } = null!;
        public string OperationCode { get; set; } = null!;
        public string MachineTypeCode { get; set; } = null!;
        public decimal Sam { get; set; }
        public decimal Quota { get; set; }
        public decimal NumberOfMachines { get; set; }
    }
}
