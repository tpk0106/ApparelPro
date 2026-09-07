namespace ApparelPro.Data.Models.Toolbar
{
    // One row per shortcut a user has pinned to the top toolbar. GroupKey and
    // ItemRouterLink mirror the routerLink values already in nav-data.ts (the
    // group's and the sub-menu's), so the frontend can match a pin straight
    // back to its nav-data entry without a second lookup table.
    public class ToolbarPin
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = null!;
        public string GroupKey { get; set; } = null!;
        public string ItemRouterLink { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
