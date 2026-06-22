using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class StockItem
    {
        public StockItem()
        {
            StockCode = "";
            ItemCode = "";
            Description = "";
        }

        // FIXED: Switched from int to string to maintain leading zero structures (e.g. "01")
        public string StockCode { get; set; } = null!;

        // FIXED: Switched from int to string to safely hold codes (e.g. "01FB", "02BT")
        public string ItemCode { get; set; } = null!;

        public string Description { get; set; } = string.Empty;

        // Fluent API Navigation Property pointing back to the updated master Stock table
        [ForeignKey("StockCode")]
        public virtual Stock Stock { get; set; } = null!;
    }
}
