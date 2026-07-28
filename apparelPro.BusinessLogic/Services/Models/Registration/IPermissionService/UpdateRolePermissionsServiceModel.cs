namespace apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService
{
    /// <summary>Replaces the full set of permissions granted to a role - the admin
    /// screen sends the complete desired set for that role on save, not individual
    /// grant/revoke deltas.</summary>
    public class UpdateRolePermissionsServiceModel
    {
        public string RoleId { get; set; } = string.Empty;
        public List<string> PermissionKeys { get; set; } = new();
    }
}
