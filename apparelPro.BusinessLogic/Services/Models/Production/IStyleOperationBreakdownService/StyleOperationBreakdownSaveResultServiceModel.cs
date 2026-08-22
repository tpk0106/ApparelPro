namespace apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService
{
    public class StyleOperationBreakdownSaveResultServiceModel
    {
        public decimal TargetDailyOutput { get; set; }
        public List<StyleOperationBreakdownServiceModel> Operations { get; set; } = new();
    }
}
