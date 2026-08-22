namespace ApparelPro.WebApi.APIModels.Production
{
    public class UpdateSectionAPIModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }
}
