using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ApparelPro.WebApi.Authorization
{
    /// <summary>
    /// Resolves [Authorize(Policy = "...")] names. Any name already registered in
    /// AuthorizationOptions (e.g. "Merchandising", "RegisteredUser" - see
    /// AuthorizationConfig.GetAuthroizationOptions) resolves exactly as it does today via
    /// the wrapped DefaultAuthorizationPolicyProvider. Any other name is treated as a
    /// Permission catalog key and resolves to a dynamic, RolePermissions-table-backed
    /// policy built from PermissionRequirement.
    ///
    /// Stage 2 groundwork only: no controller uses [Authorize(Policy = "...")] with a
    /// permission key yet - that cutover is a deliberately separate, later pass.
    /// </summary>
    public class DynamicPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                return null;
            }

            var existingPolicy = await _fallbackPolicyProvider.GetPolicyAsync(policyName);
            if (existingPolicy != null)
            {
                return existingPolicy;
            }

            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}
