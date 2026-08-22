namespace apparelPro.BusinessLogic.Services.Models.Production.ISectionService
{
    public class SectionServiceModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }
}
