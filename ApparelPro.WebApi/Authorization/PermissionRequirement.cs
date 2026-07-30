using Microsoft.AspNetCore.Authorization;

namespace ApparelPro.WebApi.Authorization
{
    /// <summary>
    /// Satisfied when the current user has at least one role that the RolePermissions
    /// table grants this Permission catalog key (see Permission.cs / RolePermission.cs).
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionKey { get; }

        public PermissionRequirement(string permissionKey)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
            {
                throw new ArgumentNullException(nameof(permissionKey));
            }

            PermissionKey = permissionKey;
        }
    }
}
