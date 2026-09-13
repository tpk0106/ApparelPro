namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService
{
    public class BoatNoteDetailServiceModel
    {
        public BoatNoteHeaderServiceModel Header { get; set; } = new();
        public List<BoatNoteCargoLineServiceModel> Lines { get; set; } = new();
    }
}
