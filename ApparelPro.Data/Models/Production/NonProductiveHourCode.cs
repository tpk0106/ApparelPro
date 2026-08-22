namespace ApparelPro.Data.Models.Production
{
    public class NonProductiveHourCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
    }
}
