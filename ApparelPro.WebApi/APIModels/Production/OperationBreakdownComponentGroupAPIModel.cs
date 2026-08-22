namespace ApparelPro.WebApi.APIModels.Production
{
    public class OperationBreakdownComponentGroupAPIModel
    {
        public string ComponentCode { get; set; } = null!;
        public string ComponentDescription { get; set; } = "";
        public List<OperationBreakdownRowAPIModel> Rows { get; set; } = new();
    }
}
