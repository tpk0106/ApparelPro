namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_coff.dbf. Shared by both Clearance Office and
    // Frontier Office fields on the CUSDEC header - legacy validates both
    // against this same table.
    public class ClearanceOffice
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
