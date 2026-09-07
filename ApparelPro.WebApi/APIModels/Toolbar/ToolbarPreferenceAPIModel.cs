namespace ApparelPro.WebApi.APIModels.Toolbar
{
    public class ToolbarPreferenceAPIModel
    {
        public bool IsEnabled { get; set; } = true;
        public bool IsDefault { get; set; }
        public List<ToolbarPinAPIModel> Pins { get; set; } = new();
    }
}
