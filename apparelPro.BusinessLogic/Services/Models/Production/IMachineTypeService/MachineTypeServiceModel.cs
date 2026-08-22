namespace apparelPro.BusinessLogic.Services.Models.Production.IMachineTypeService
{
    public class MachineTypeServiceModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsManual { get; set; }
    }
}
