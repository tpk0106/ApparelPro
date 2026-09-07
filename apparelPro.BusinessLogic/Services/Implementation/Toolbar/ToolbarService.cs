using apparelPro.BusinessLogic.Services.Models.Toolbar.IToolbarService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Toolbar;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Toolbar
{
    // Owns the per-user quick-access toolbar: whether it's shown at all
    // (ToolbarPreference) and which nav-data.ts shortcuts are pinned
    // (ToolbarPin). Keyed by the authenticated user's email - the only
    // identity claim this app issues (see StyleDetailsController) - rather
    // than a Users.Id, since Users lives in a separate UserIdentityDbContext.
    public class ToolbarService : IToolbarService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public ToolbarService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ToolbarPreferenceServiceModel> GetPreferencesAsync(string userEmail)
        {
            var preference = await _apparelProDbContext.ToolbarPreferences
                .FirstOrDefaultAsync(p => p.UserEmail == userEmail);

            var pins = await _apparelProDbContext.ToolbarPins
                .Where(p => p.UserEmail == userEmail)
                .OrderBy(p => p.GroupKey).ThenBy(p => p.SortOrder)
                .ToListAsync();

            return new ToolbarPreferenceServiceModel
            {
                IsEnabled = preference?.IsEnabled ?? true,
                IsDefault = preference == null,
                Pins = _mapper.Map<List<ToolbarPinServiceModel>>(pins)
            };
        }

        public async Task<ToolbarPreferenceServiceModel> SavePreferencesAsync(
            string userEmail, SaveToolbarPreferenceServiceModel saveToolbarPreferenceServiceModel)
        {
            var preference = await _apparelProDbContext.ToolbarPreferences
                .FirstOrDefaultAsync(p => p.UserEmail == userEmail);

            if (preference == null)
            {
                preference = new ToolbarPreference { UserEmail = userEmail };
                _apparelProDbContext.ToolbarPreferences.Add(preference);
            }
            preference.IsEnabled = saveToolbarPreferenceServiceModel.IsEnabled;

            var existingPins = await _apparelProDbContext.ToolbarPins
                .Where(p => p.UserEmail == userEmail)
                .ToListAsync();
            _apparelProDbContext.ToolbarPins.RemoveRange(existingPins);

            var newPins = saveToolbarPreferenceServiceModel.Pins
                .Select(p => new ToolbarPin
                {
                    UserEmail = userEmail,
                    GroupKey = p.GroupKey,
                    ItemRouterLink = p.ItemRouterLink,
                    SortOrder = p.SortOrder
                })
                .ToList();
            await _apparelProDbContext.ToolbarPins.AddRangeAsync(newPins);

            await _apparelProDbContext.SaveChangesAsync();

            return await GetPreferencesAsync(userEmail);
        }
    }
}
