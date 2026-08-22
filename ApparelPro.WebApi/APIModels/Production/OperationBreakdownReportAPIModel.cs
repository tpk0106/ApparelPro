namespace ApparelPro.WebApi.APIModels.Production
{
    public class OperationBreakdownReportAPIModel
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
        public List<OperationBreakdownComponentGroupAPIModel> Groups { get; set; } = new();
    }
}
