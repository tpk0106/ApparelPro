using apparelPro.BusinessLogic.Services.Models.SystemConfiguration.ISystemParameterService;
using ApparelPro.Data;
using ApparelPro.Data.Models.SystemConfiguration;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace apparelPro.BusinessLogic.Services.Implementation.SystemConfiguration
{
    public class SystemParameterService : ISystemParameterService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private const string CacheKeyPrefix = "system-parameter:";

        public SystemParameterService(ApparelProDbContext apparelProDbContext, IMapper mapper, IMemoryCache memoryCache)
        {
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<SystemParameterServiceModel>> GetAllParametersAsync()
        {
            var parameters = await _apparelProDbContext.SystemParameters.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<SystemParameterServiceModel>>(parameters);
        }

        public async Task<string?> GetValueAsync(string parameterKey)
        {
            var cacheKey = CacheKeyPrefix + parameterKey;
            if (_memoryCache.TryGetValue(cacheKey, out string? cachedValue))
            {
                return cachedValue;
            }

            var parameter = await _apparelProDbContext.SystemParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParameterKey == parameterKey);

            var value = parameter?.Value;
            _memoryCache.Set(cacheKey, value, TimeSpan.FromMinutes(5));
            return value;
        }

        public async Task<bool> GetBoolValueAsync(string parameterKey, bool defaultValue = false)
        {
            var value = await GetValueAsync(parameterKey);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public async Task UpdateParameterAsync(string parameterKey, string value)
        {
            var parameter = await _apparelProDbContext.SystemParameters
                .FirstOrDefaultAsync(p => p.ParameterKey == parameterKey);

            if (parameter == null)
            {
                throw new KeyNotFoundException($"System parameter '{parameterKey}' was not found.");
            }

            parameter.Value = value;
            await _apparelProDbContext.SaveChangesAsync();

            _memoryCache.Remove(CacheKeyPrefix + parameterKey);
        }
    }
}
