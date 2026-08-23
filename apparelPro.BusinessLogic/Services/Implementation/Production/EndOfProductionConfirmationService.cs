using apparelPro.BusinessLogic.Services.Models.Production.IEndOfProductionConfirmationService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    // Replicates PR_ENDPR.PRG's "END OF PRODUCTION CONFIRMATION" screen -
    // see EndOfProductionStatusServiceModel for the exact semantics.
    public class EndOfProductionConfirmationService : IEndOfProductionConfirmationService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public EndOfProductionConfirmationService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<EndOfProductionStatusServiceModel> GetStatusAsync(
            int buyerCode, string order, int typeCode, string styleCode)
        {
            var style = await _apparelProDbContext.Styles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order &&
                                           s.TypeCode == typeCode && s.StyleCode == styleCode);

            // Mirrors legacy's "Style not entered" error box.
            if (style == null)
                throw new InvalidOperationException("Style not entered.");

            var hasProductionEntries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .AnyAsync(e => e.BuyerCode == buyerCode && e.Order == order &&
                               e.TypeCode == typeCode && e.StyleCode == styleCode);

            return new EndOfProductionStatusServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                CurrentProductionEndDate = style.ProductionEndDate,
                HasProductionEntries = hasProductionEntries
            };
        }

        public async Task<EndOfProductionStatusServiceModel> ConfirmAsync(
            int buyerCode, string order, int typeCode, string styleCode, DateOnly endDate)
        {
            var style = await _apparelProDbContext.Styles
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order &&
                                           s.TypeCode == typeCode && s.StyleCode == styleCode);

            // Mirrors legacy's "Style not entered" error box.
            if (style == null)
                throw new InvalidOperationException("Style not entered.");

            var hasProductionEntries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .AnyAsync(e => e.BuyerCode == buyerCode && e.Order == order &&
                               e.TypeCode == typeCode && e.StyleCode == styleCode);

            // Mirrors legacy's "No Daily Prodction Entries available." error box.
            if (!hasProductionEntries)
                throw new InvalidOperationException("No Daily Production Entries available.");

            style.ProductionEndDate = endDate;
            await _apparelProDbContext.SaveChangesAsync();

            return new EndOfProductionStatusServiceModel
            {
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                CurrentProductionEndDate = style.ProductionEndDate,
                HasProductionEntries = hasProductionEntries
            };
        }
    }
}
