namespace ApparelPro.WebApi.APIModels.Production
{
    public class CreateMachineTypeAPIModel
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsManual { get; set; }
    }
}
