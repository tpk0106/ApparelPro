namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_tran.dbf (TRPT code)
    public class TransportMode
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
