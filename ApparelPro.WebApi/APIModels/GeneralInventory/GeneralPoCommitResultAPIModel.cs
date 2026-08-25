namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPoCommitResultAPIModel
    {
        public string PoNumber { get; set; } = null!;
        public List<string> Warnings { get; set; } = new();
    }
}
