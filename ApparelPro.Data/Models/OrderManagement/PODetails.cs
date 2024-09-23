namespace ApparelPro.Data.Models.OrderManagement
{
    public class PODetails
    {
        public int PONo { get; set; }
        public int Buyer { get; set; }
        public string Order { get; set; }
        public int Type { get; set; }
        public string Style { get; set; }
        public string ItemCode { get; set; }
        public string RefNo { get; set; }
        public string OrderUnit { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime ExportDate { get; set; }
        public string LCNo { get; set; }
        public int Balance { get; set; }
    }
}
