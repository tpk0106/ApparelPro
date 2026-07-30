namespace ApparelPro.WebApi.Authorization
{
    /// <summary>
    /// In-memory, refreshable view of the RolePermissions table, keyed by Permission.Key,
    /// so PermissionAuthorizationHandler doesn't hit the database on every authorized
    /// request. Call Invalidate() after the future Access Rights admin screen saves a
    /// change, so the very next request picks up the new grants.
    /// </summary>
    public interface IRolePermissionCache
    {
        Task<IReadOnlyList<string>> GetRoleNamesForPermissionAsync(string permissionKey);
        void Invalidate();
    }
}
