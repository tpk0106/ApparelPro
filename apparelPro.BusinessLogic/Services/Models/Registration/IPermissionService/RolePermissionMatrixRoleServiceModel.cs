namespace apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService
{
    /// <summary>One row of the future Access Rights admin grid: a role, and the set
    /// of permission keys currently granted to it.</summary>
    public class RolePermissionMatrixRoleServiceModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<string> GrantedPermissionKeys { get; set; } = new();
    }
}
