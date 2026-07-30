using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ApparelPro.WebApi.Authorization
{
    /// <summary>
    /// Singleton cache backing IRolePermissionCache. Registered as a singleton (shared
    /// across every request) but resolves a scoped UserIdentityDbContext per rebuild via
    /// IServiceScopeFactory, since a singleton cannot directly depend on a scoped service.
    /// </summary>
    public class RolePermissionCache : IRolePermissionCache
    {
        private const string CacheKey = "ApparelPro:RolePermissionCache:PermissionToRoleNames";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private static readonly SemaphoreSlim RebuildLock = new(1, 1);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _memoryCache;

        public RolePermissionCache(IServiceScopeFactory serviceScopeFactory, IMemoryCache memoryCache)
        {
            if (serviceScopeFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }
            if (memoryCache == null)
            {
                throw new ArgumentNullException(nameof(memoryCache));
            }

            _serviceScopeFactory = serviceScopeFactory;
            _memoryCache = memoryCache;
        }

        public async Task<IReadOnlyList<string>> GetRoleNamesForPermissionAsync(string permissionKey)
        {
            var map = await GetOrBuildMapAsync();
            if (map.TryGetValue(permissionKey, out var roleNames))
            {
                return roleNames;
            }

            return Array.Empty<string>();
        }

        public void Invalidate()
        {
            _memoryCache.Remove(CacheKey);
        }

        private async Task<Dictionary<string, List<string>>> GetOrBuildMapAsync()
        {
            if (_memoryCache.TryGetValue(CacheKey, out Dictionary<string, List<string>>? cached) && cached != null)
            {
                return cached;
            }

            await RebuildLock.WaitAsync();
            try
            {
                if (_memoryCache.TryGetValue(CacheKey, out cached) && cached != null)
                {
                    return cached;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var userIdentityDbContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();

                var rows = await (
                    from rolePermission in userIdentityDbContext.RolePermissions
                    join role in userIdentityDbContext.Roles on rolePermission.RoleId equals role.Id
                    join permission in userIdentityDbContext.Permissions on rolePermission.PermissionId equals permission.Id
                    select new { permission.Key, role.Name }
                ).ToListAsync();

                var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var row in rows)
                {
                    if (string.IsNullOrEmpty(row.Name))
                    {
                        continue;
                    }

                    if (!map.TryGetValue(row.Key, out var roleNames))
                    {
                        roleNames = new List<string>();
                        map[row.Key] = roleNames;
                    }

                    roleNames.Add(row.Name);
                }

                _memoryCache.Set(CacheKey, map, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration
                });

                return map;
            }
            finally
            {
                RebuildLock.Release();
            }
        }
    }
}
