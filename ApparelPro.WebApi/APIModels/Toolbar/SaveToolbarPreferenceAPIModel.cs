namespace ApparelPro.WebApi.APIModels.Toolbar
{
    public class SaveToolbarPreferenceAPIModel
    {
        public bool IsEnabled { get; set; } = true;
        public List<ToolbarPinAPIModel> Pins { get; set; } = new();
    }
}
