using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApparelPro.WebApi.Authorization
{
    /// <summary>
    /// Succeeds a PermissionRequirement when the current user holds at least one role
    /// (via the ClaimTypes.Role claims MapInboundClaims = true remaps in Program.cs)
    /// that the RolePermissions table grants the requirement's permission key.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRolePermissionCache _rolePermissionCache;

        public PermissionAuthorizationHandler(IRolePermissionCache rolePermissionCache)
        {
            if (rolePermissionCache == null)
            {
                throw new ArgumentNullException(nameof(rolePermissionCache));
            }

            _rolePermissionCache = rolePermissionCache;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (userRoles.Count == 0)
            {
                return;
            }

            var allowedRoles = await _rolePermissionCache.GetRoleNamesForPermissionAsync(requirement.PermissionKey);
            if (allowedRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }
    }
}
