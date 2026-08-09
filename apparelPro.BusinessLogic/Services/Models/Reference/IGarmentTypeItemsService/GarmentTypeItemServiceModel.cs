namespace apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeItemsService
{
    public class GarmentTypeItemServiceModel
    {
        public int Id { get; set; }
        public int GarmentTypeId { get; set; }
        public string GarmentTypeName { get; set; } = "";
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string ItemDescription { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
