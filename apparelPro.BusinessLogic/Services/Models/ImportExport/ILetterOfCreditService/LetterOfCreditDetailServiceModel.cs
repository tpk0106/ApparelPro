namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditService
{
    public class LetterOfCreditDetailServiceModel
    {
        public LetterOfCreditHeaderServiceModel Header { get; set; } = null!;
        public List<LetterOfCreditLineServiceModel> Lines { get; set; } = new();
    }
}
