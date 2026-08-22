namespace ApparelPro.WebApi.APIModels.Production
{
    public class CreateSectionAPIModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }
}
