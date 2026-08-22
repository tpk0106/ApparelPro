namespace ApparelPro.WebApi.APIModels.Production
{
    public class OperationBreakdownRowAPIModel
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
}
