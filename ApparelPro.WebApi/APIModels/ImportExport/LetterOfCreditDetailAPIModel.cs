namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class LetterOfCreditDetailAPIModel
    {
        public LetterOfCreditHeaderAPIModel Header { get; set; } = null!;
        public List<LetterOfCreditLineAPIModel> Lines { get; set; } = new();
    }
}
