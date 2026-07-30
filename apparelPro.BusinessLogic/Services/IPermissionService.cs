using apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPermissionService
    {
        // Full catalog for the admin screen's column headers / permission list.
        Task<List<PermissionServiceModel>> GetPermissionCatalogAsync();

        // One row per existing ASP.NET Identity role, each carrying the set of
        // permission keys currently granted to it - the admin screen's grid.
        Task<List<RolePermissionMatrixRoleServiceModel>> GetRolePermissionMatrixAsync();

        // Replaces the FULL set of permissions granted to a role (delete-then-insert),
        // not an incremental grant/revoke - matches how the admin screen saves.
        Task UpdateRolePermissionsAsync(UpdateRolePermissionsServiceModel model);

        // Raw role-name -> permission-key lookup, used by RolePermissionCache to
        // rebuild its in-memory snapshot. Not for controller use.
        Task<Dictionary<string, List<string>>> GetPermissionKeysByRoleNameAsync();

        // Idempotent: inserts any catalog rows from DEFAULT_CATALOG that don't
        // already exist (matched by Key), and grants any default role mappings that
        // don't already exist (matched by RoleId+PermissionId) - safe to call
        // repeatedly, e.g. every time a new Permission is added to the catalog in
        // code. Never revokes or removes an existing grant, even if a default
        // mapping was later hand-edited via the admin screen.
        Task SeedDefaultCatalogAsync();

        // Explicit, opt-in removal of a fixed, hand-maintained list of catalog keys
        // that a controller cutover has retired (e.g. "bank" -> "bank-view" +
        // "bank-manage"). Deletes the Permission row and any RolePermission grants
        // pointing at it. Deliberately separate from SeedDefaultCatalogAsync so a key
        // is only ever removed because someone explicitly listed it as obsolete, not
        // because it silently fell out of DEFAULT_CATALOG.
        Task RemoveObsoleteCatalogEntriesAsync();
    }
}
