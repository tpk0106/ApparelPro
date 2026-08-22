namespace ApparelPro.Data.Models.Production
{
    public class Operation
    {
        public int Id { get; set; }
        public string OperationCode { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
    }
}
