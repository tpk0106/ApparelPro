namespace ApparelPro.Data.Models.References
{
    public class StockItem
    {
        public StockItem() { StockCode = 0; ItemCode = 0; Description = ""; Stock = new Stock();  }
        public int StockCode { get; set; }
        public int ItemCode { get; set; }
        public string Description { get; set; } = string.Empty;

        public Stock Stock { get; set; } 
    }
}
