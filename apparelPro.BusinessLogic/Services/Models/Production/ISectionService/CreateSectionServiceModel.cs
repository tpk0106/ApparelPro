namespace apparelPro.BusinessLogic.Services.Models.Production.ISectionService
{
    public class CreateSectionServiceModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }
}
