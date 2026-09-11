namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SaveLetterOfCreditAPIModel
    {
        public LetterOfCreditHeaderAPIModel Header { get; set; } = null!;
        public List<LetterOfCreditLineAPIModel> Lines { get; set; } = new();
    }
}
