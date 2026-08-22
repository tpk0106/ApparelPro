namespace ApparelPro.Data.Models.Production
{
    // CALENDER.DBF - a row's presence marks that date as non-working. Used
    // by the automatic line-allocation scheduler to skip holidays when
    // counting production days.
    public class Holiday
    {
        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
