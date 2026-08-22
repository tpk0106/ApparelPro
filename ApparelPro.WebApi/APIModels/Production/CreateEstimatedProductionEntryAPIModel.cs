namespace ApparelPro.WebApi.APIModels.Production
{
    public class CreateEstimatedProductionEntryAPIModel
    {
        public DateOnly Date { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
