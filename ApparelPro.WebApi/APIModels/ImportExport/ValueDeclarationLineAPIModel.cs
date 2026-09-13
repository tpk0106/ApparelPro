namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class ValueDeclarationLineAPIModel
    {
        public int Id { get; set; }
        public int ValueDeclarationHeaderId { get; set; }
        public int ItemNo { get; set; }
        public string Description { get; set; } = "";
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Size { get; set; }
        public string? CountryOfOriginCode { get; set; }
        public string? UnitCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal Value { get; set; }
        public string? HsCode { get; set; }
        public decimal? Weight { get; set; }
    }
}
