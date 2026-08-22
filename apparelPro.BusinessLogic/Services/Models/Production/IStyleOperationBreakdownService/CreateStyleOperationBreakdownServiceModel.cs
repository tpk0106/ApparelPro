namespace apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService
{
    // Quota and NumberOfMachines are deliberately absent - both are fully
    // server-computed by OperationBreakdownBalanceCalculator during
    // BulkSaveAndRecalculateAsync, matching PR_OPD2.PRG where the operator
    // never types either value directly (Quota is derived the instant SAM
    // is entered; NumberOfMachines only gets set by the whole-style
    // "Saving Entries" recalculation pass).
    public class CreateStyleOperationBreakdownServiceModel
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
    }
}
