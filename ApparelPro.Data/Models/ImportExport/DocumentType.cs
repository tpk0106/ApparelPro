namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_docu.dbf - genuinely composite-keyed (DocNo+DocType):
    // one document category (DocNo) can have several DocType rows under it.
    public class DocumentType
    {
        public int Id { get; set; }
        public string DocNo { get; set; } = "";
        public string DocTypeCode { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
