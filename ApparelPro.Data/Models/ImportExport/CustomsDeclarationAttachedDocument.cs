namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_cusd4.dbf (attached documents). Keyed by
    // CusNo+DocNo+DocType. The legacy INVO_NO column is dead (already
    // commented out in the .PRG source) and is not carried forward.
    public class CustomsDeclarationAttachedDocument
    {
        public int Id { get; set; }
        public string CusNo { get; set; } = "";
        public string DocNo { get; set; } = "";
        public string DocTypeCode { get; set; } = "";
    }
}
