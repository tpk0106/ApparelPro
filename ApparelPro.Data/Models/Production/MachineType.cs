namespace ApparelPro.Data.Models.Production
{
    public class MachineType
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsManual { get; set; }
    }
}
