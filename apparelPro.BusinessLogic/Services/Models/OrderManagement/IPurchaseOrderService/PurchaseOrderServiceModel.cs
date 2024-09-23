namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService
{
    public class PurchaseOrderServiceModel
    {
        public int BuyerCode { get; set; }
        public string? Buyer { get; set; }
        public string? Order { get; set; }
        public DateTime OrderDate { get; set; }
        public int GarmentType { get; set; }
        public string? GarmentTypeName { get; set; }
        public string? CountryCode { get; set; }
        public string? UnitCode { get; set; }
        public decimal TotalQuantity { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Season { get; set; }
        public string? BasisCode { get; set; }
        public decimal BasisValue { get; set; }
    }
}
