namespace apparelPro.BusinessLogic.Services.Models.Toolbar.IToolbarService
{
    public class ToolbarPreferenceServiceModel
    {
        public bool IsEnabled { get; set; } = true;

        // True when the user has never saved a preference yet - the frontend
        // uses this to fall back to nav-data.ts's own `pinned: true` defaults
        // instead of showing an empty toolbar for a first-time user.
        public bool IsDefault { get; set; }
        public List<ToolbarPinServiceModel> Pins { get; set; } = new();
    }
}
