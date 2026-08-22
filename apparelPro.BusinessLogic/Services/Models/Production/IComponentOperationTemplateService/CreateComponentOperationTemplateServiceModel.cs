namespace apparelPro.BusinessLogic.Services.Models.Production.IComponentOperationTemplateService
{
    public class CreateComponentOperationTemplateServiceModel
    {
        public string ComponentCode { get; set; } = null!;
        public int OperationSequence { get; set; }
        public string OperationCode { get; set; } = null!;
        public string MachineTypeCode { get; set; } = null!;
        public decimal Sam { get; set; }
        public decimal NumberOfMachines { get; set; }
    }
}
