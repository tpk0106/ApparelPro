namespace apparelPro.BusinessLogic.Services.Models.Production.IOperationBreakdownReportService
{
    // Replicates PR_REP1.PRG's "OPERATION BREAKDOWN" report (Reports ->
    // Operation Breakdown). Reads the already-migrated StyleOperationBreakdown
    // rows for a Buyer/Order/Type/Style and joins in Operation/GarmentComponent/
    // MachineType descriptions plus the factory-wide Eff1/Eff2/WorkHoursPerDay
    // System Parameters. This is the first report to use Eff1 - every other
    // consumer of StyleOperationBreakdown (balance recalculation) only uses
    // Eff2. NumberOfMachines/Quota are read directly off the stored row, not
    // recalculated - legacy's own "no_ops = xoutput1/perc" live-calculation
    // was commented out and replaced by reading the stored field, and the
    // whole total-SAM/output projection block that fed it was never actually
    // printed (verified against the legacy source), so it's omitted here too.
    public class OperationBreakdownRowServiceModel
    {
        public int DisplayOperationNo { get; set; }
        public string OperationCode { get; set; } = null!;
        public string OperationDescription { get; set; } = "";
        public string MachineTypeCode { get; set; } = null!;
        public decimal Sam { get; set; }
        public decimal QuotaAt100 { get; set; }
        public decimal QuotaAtEff1 { get; set; }
        public decimal QuotaPcsPer2HrsAtEff1 { get; set; }
        public decimal QuotaAtEff2 { get; set; }
        public decimal QuotaPcsPer2HrsAtEff2 { get; set; }
        public decimal NumberOfMachines { get; set; }
        public decimal NumberOfOperators { get; set; }
    }

    public class OperationBreakdownComponentGroupServiceModel
    {
        public string ComponentCode { get; set; } = null!;
        public string ComponentDescription { get; set; } = "";
        public List<OperationBreakdownRowServiceModel> Rows { get; set; } = new();
    }

    public class OperationBreakdownReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public decimal Eff1Percent { get; set; }
        public decimal Eff2Percent { get; set; }
        public decimal WorkHoursPerDay { get; set; }
        public List<OperationBreakdownComponentGroupServiceModel> Groups { get; set; } = new();
    }
}
