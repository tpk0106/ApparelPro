namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService
{
    public class SaveBoatNoteServiceModel
    {
        public BoatNoteHeaderServiceModel Header { get; set; } = new();
        public List<BoatNoteCargoLineServiceModel> Lines { get; set; } = new();
    }
}
