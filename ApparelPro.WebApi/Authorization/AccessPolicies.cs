namespace ApparelPro.WebApi.Authorization
{
    /// <summary>
    /// Single source of truth for role-based authorization strings. Reference these
    /// instead of typing raw comma-separated role lists on [Authorize] attributes -
    /// the Buyers-list incident (a note-type role silently missing from a shared
    /// endpoint) happened because that string was duplicated by hand across ~15
    /// controllers with no single place to check or update it.
    /// </summary>
    public static class AccessPolicies
    {
        /// <summary>Standard access for every day-to-day Orderwise Inventory note
        /// (GIN, GRN, RTN, GTN, SRN, DGN, STRN) and the reference-data screens they
        /// depend on (Buyers, Departments, Garment Types, Item Features, etc).</summary>
        public const string OrderwiseInventoryStandard =
            "Inventory, Merchandiser, Merchandiser Manager, Order Entry Operator";

        /// <summary>Merchandising-only reference data (Currency, Garment Type detail,
        /// Material Consumption, Color/Size Breakdown).</summary>
        public const string MerchandisingOnly =
            "Merchandiser, Merchandiser Manager";

        /// <summary>Stock Adjustment Note (SAN) - directly overwrites physical stock
        /// counts with no ceiling check, restricted to higher-authority roles only.</summary>
        public const string StoreManagementOnly =
            "Store Manager, Administrator";

        /// <summary>System-level actions (revoking refresh tokens, future admin-only
        /// screens).</summary>
        public const string AdministratorOnly =
            "Administrator";
    }
}
