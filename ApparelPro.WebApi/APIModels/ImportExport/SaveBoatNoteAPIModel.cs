namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SaveBoatNoteAPIModel
    {
        public BoatNoteHeaderAPIModel Header { get; set; } = new();
        public List<BoatNoteCargoLineAPIModel> Lines { get; set; } = new();
    }
}
