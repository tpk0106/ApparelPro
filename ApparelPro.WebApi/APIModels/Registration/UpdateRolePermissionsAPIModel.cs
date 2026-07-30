namespace ApparelPro.WebApi.APIModels.Registration
{
    public class UpdateRolePermissionsAPIModel
    {
        public string RoleId { get; set; } = string.Empty;
        public List<string> PermissionKeys { get; set; } = new();
    }
}
