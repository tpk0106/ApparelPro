namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_cpro.dbf (CPC - Customs Procedure Code)
    public class CustomsProcedureCode
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
