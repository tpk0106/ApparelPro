namespace ApparelPro.Data.Models.OrderManagement
{
    public class PODetails
    {
        public int Id { get; set; } // Maintain an internal database auto-increment identity

        // FIXED: Switched from int to string to perfectly match Clipper's Character (C 6) type!
        public string PONumber { get; set; } = null!;

        public int Buyer { get; set; }
        public string Order { get; set; } = null!;
        public int Type { get; set; }
        public string Style { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // Your 22-character compound key
        public string? RefNo { get; set; }
        public string OrderUnit { get; set; } = null!;
        public decimal OrderQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime? ExportDate { get; set; }
        public string? LCNo { get; set; }

        // FIXED: Switched from int to decimal to handle fractional yardages/metres safely
        public decimal Balance { get; set; }
    }
}
