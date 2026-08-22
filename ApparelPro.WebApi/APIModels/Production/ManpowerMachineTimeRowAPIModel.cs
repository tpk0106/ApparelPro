namespace ApparelPro.WebApi.APIModels.Production
{
    public class ManpowerMachineTimeRowAPIModel
    {
        public string MachineTypeCode { get; set; } = null!;
        public string MachineTypeDescription { get; set; } = "";
        public decimal TotalSam { get; set; }
    }
}
