using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.SystemConfiguration
{
    public class SystemParameterLookupService : ISystemParameterLookupService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public SystemParameterLookupService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<string> GetValueAsync(string parameterKey, string defaultValue)
        {
            var value = await _apparelProDbContext.SystemParameters
                .AsNoTracking()
                .Where(p => p.ParameterKey == parameterKey)
                .Select(p => p.Value)
                .FirstOrDefaultAsync();

            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
