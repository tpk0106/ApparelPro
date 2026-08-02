namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService
{
    public class StyleTotalsServiceModel
    {
        // Sum of every style's Quantity for this buyer+order, each converted into MainUnit
        // (mirrors legacy OD_STY1.PRG's running "TOTAL" - convert(unit, xunit, qty)).
        public decimal TotalQuantity { get; set; }
        public string MainUnit { get; set; } = null!;
        public decimal OrderTotalQuantity { get; set; }
        public bool ExceedsOrderQuantity { get; set; }
    }
}
