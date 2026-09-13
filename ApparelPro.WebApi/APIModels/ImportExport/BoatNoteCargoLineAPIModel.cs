namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class BoatNoteCargoLineAPIModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public int LineNo { get; set; }
        public string ContainerNo { get; set; } = "";
        public string? SealNo { get; set; }
        public string PackageQuantity { get; set; } = "";
        public string Description { get; set; } = "";
        public string? HsCode { get; set; }
        public decimal GrossWeight { get; set; }
        public string WeightUnit { get; set; } = "KG";
    }
}
