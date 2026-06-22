namespace ApparelPro.Data.Models.References
{
    public class Stock
    {
        // FIXED: Switched from int identity to a clean explicit string primary key 
        // to preserve padded leading zero values (e.g. "01", "02") safely!
        public string StockCode { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
    }
}
