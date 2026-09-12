namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_pay.dbf
    public class PaymentTerm
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
