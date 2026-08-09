namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class SaveSubContractResultAPIModel
    {
        public SubContractAPIModel SubContract { get; set; } = null!;
        public string? QuantityWarning { get; set; }
    }
}
