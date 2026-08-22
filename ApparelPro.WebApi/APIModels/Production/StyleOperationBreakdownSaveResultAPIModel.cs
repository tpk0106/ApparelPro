namespace ApparelPro.WebApi.APIModels.Production
{
    public class StyleOperationBreakdownSaveResultAPIModel
    {
        public decimal TargetDailyOutput { get; set; }
        public List<StyleOperationBreakdownAPIModel> Operations { get; set; } = new();
    }
}
