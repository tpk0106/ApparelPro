namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class OrderPipelineResultAPIModel
    {
        public List<OrderPipelineRowAPIModel> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int[] StageCounts { get; set; } = new int[7];
        public int OverdueCount { get; set; }
    }
}
