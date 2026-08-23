namespace ApparelPro.WebApi.APIModels.Production
{
    public class EndOfProductionStatusAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public DateOnly? CurrentProductionEndDate { get; set; }
        public bool HasProductionEntries { get; set; }
    }
}
