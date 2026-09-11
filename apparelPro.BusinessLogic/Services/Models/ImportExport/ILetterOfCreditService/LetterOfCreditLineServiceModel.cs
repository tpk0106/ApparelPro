namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditService
{
    public class LetterOfCreditLineServiceModel
    {
        public int Id { get; set; }
        public string BankCode { get; set; } = "";
        public string LcNo { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Currency { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public string? BtnNo { get; set; }
    }
}
