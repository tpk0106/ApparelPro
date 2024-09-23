namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CreatePOAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public DateTime OrderDate { get; set; }
        public string GarmentType { get; set; }
        public string CountryCode { get; set; }
        public string UnitCode { get; set; }
        public decimal TotalQuantity { get; set; }
        public string CurrencyCode { get; set; }
        public string Season { get; set; }
        public string BasisCode { get; set; }
        public decimal BasisValue { get; set; }
    }
}
