namespace apparelPro.BusinessLogic.Services.Models.Toolbar.IToolbarService
{
    public class SaveToolbarPreferenceServiceModel
    {
        public bool IsEnabled { get; set; } = true;
        public List<ToolbarPinServiceModel> Pins { get; set; } = new();
    }
}
