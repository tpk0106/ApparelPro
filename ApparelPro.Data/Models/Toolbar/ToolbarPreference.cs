namespace ApparelPro.Data.Models.Toolbar
{
    // One row per user - whether the top quick-access toolbar is shown at all.
    // Pin selections live separately in ToolbarPin, keyed by the same UserEmail.
    public class ToolbarPreference
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = null!;
        public bool IsEnabled { get; set; } = true;
    }
}
