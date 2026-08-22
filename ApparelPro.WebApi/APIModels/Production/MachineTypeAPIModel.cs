namespace ApparelPro.WebApi.APIModels.Production
{
    public class MachineTypeAPIModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsManual { get; set; }
    }
}
