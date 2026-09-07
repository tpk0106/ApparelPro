namespace ApparelPro.WebApi.APIModels.Toolbar
{
    public class ToolbarPinAPIModel
    {
        public string GroupKey { get; set; } = null!;
        public string ItemRouterLink { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
