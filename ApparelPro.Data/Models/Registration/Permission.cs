namespace ApparelPro.Data.Models.Registration
{
    /// <summary>
    /// One row per protected section (e.g. "gin", "san", "buyers") - the catalog that
    /// RolePermission maps roles onto. This is what the future Access Rights admin
    /// screen lists and lets an administrator grant/revoke per role, without any
    /// code change or redeploy.
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }

        /// <summary>Stable machine key referenced by [Authorize(Policy = Key)] on
        /// controllers - e.g. "san", "gin", "buyers". Never shown to end users.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Human-readable label for the admin screen, e.g. "Stock Adjustment
        /// Note".</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Grouping for the admin screen's roles x permissions grid, e.g.
        /// "Orderwise Inventory", "Reference Data".</summary>
        public string Category { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
