namespace ApparelPro.WebApi.APIModels.Registration
{
    public class RolePermissionMatrixRoleAPIModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<string> GrantedPermissionKeys { get; set; } = new();
    }
}
