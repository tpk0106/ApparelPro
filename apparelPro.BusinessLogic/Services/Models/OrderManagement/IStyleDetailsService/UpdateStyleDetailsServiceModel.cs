namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService
{
    public class UpdateStyleDetailsServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }        
        public int TypeCode { get; set; }
        public string StyleCode { get; set; }
        public string? Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }      
    }
}
