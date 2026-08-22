namespace ApparelPro.Data.Models.Production
{
    public class GarmentComponent
    {
        public int Id { get; set; }
        public string ComponentCode { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
    }
}
