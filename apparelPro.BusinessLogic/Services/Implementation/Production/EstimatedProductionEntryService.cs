using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionEntryService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class EstimatedProductionEntryService : IEstimatedProductionEntryService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public EstimatedProductionEntryService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<List<EstimatedProductionEntryServiceModel>> GetByLineAsync(
            int buyerCode, string order, int typeCode, string styleCode, string lineCode)
        {
            var entities = await _apparelProDbContext.EstimatedProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode && e.LineCode == lineCode)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return _mapper.Map<List<EstimatedProductionEntryServiceModel>>(entities);
        }

        public async Task<List<EstimatedProductionEntryServiceModel>> BulkSaveAsync(
            int buyerCode, string order, int typeCode, string styleCode, string lineCode,
            List<CreateEstimatedProductionEntryServiceModel> records)
        {
            // Matches legacy's "Line not allocated for [Buyer/Order/Type/Style]"
            // check (PR_ESTD1.PRG line 114-119) - estimates can only be
            // entered once the line is actually allocated to this style.
            var isLineAllocated = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .AnyAsync(a => a.BuyerCode == buyerCode && a.Order == order &&
                               a.TypeCode == typeCode && a.StyleCode == styleCode &&
                               a.LineCode == lineCode);

            if (!isLineAllocated)
            {
                throw new InvalidOperationException(
                    $"Line '{lineCode}' is not allocated for this Buyer/Order/Type/Style.");
            }

            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var existingEntries = await _apparelProDbContext.EstimatedProductionEntries
                    .Where(e => e.BuyerCode == buyerCode && e.Order == order &&
                                e.TypeCode == typeCode && e.StyleCode == styleCode && e.LineCode == lineCode)
                    .ToListAsync();

                if (existingEntries.Count > 0)
                {
                    _apparelProDbContext.EstimatedProductionEntries.RemoveRange(existingEntries);
                }

                var newEntities = records.Select(r => new EstimatedProductionEntry
                {
                    BuyerCode = buyerCode,
                    Order = order,
                    TypeCode = typeCode,
                    StyleCode = styleCode,
                    LineCode = lineCode,
                    Date = r.Date,
                    Unit = r.Unit,
                    Quantity = r.Quantity
                }).ToList();

                if (newEntities.Count > 0)
                {
                    await _apparelProDbContext.EstimatedProductionEntries.AddRangeAsync(newEntities);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return _mapper.Map<List<EstimatedProductionEntryServiceModel>>(newEntities);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
