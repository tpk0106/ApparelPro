namespace ApparelPro.Data.Models.References
{
    // Maps legacy od_sea - the season master (sea_cd + desc) referenced by
    // PurchaseOrder.Season, used to resolve a display description for the
    // Year/Season Wise Orders report.
    public class Season
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
