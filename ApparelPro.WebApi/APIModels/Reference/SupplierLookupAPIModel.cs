namespace ApparelPro.WebApi.APIModels.Reference
{
    public class SupplierLookupAPIModel
    {
        public int SupplierCode { get; set; } // Matches your database primary key integer
        public string Name { get; set; } = string.Empty;
    }
}
