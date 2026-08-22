using apparelPro.BusinessLogic.Services.Models.Production.IStyleComponentBreakdownService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class StyleComponentBreakdownService : IStyleComponentBreakdownService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public StyleComponentBreakdownService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<List<StyleComponentBreakdownServiceModel>> GetBreakdownByStyleAsync(
            int buyerCode, string order, int typeCode, string styleCode)
        {
            var records = await _apparelProDbContext.StyleComponentBreakdowns
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order &&
                            d.TypeCode == typeCode && d.StyleCode == styleCode)
                .OrderBy(d => d.ComponentSequence)
                .ToListAsync();

            return _mapper.Map<List<StyleComponentBreakdownServiceModel>>(records);
        }

        // SRP: this class only owns Component Breakdown persistence. The
        // downstream line-balancing recalculation lives entirely in
        // StyleOperationBreakdownService and is never triggered from here -
        // PR_OPD1.PRG's component list save has no such side effect either.
        public async Task<bool> BulkSaveComponentBreakdownAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            List<CreateStyleComponentBreakdownServiceModel> records)
        {
            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var existingRecords = await _apparelProDbContext.StyleComponentBreakdowns
                    .Where(d => d.BuyerCode == buyerCode && d.Order == order &&
                                d.TypeCode == typeCode && d.StyleCode == styleCode)
                    .ToListAsync();

                if (existingRecords.Count > 0)
                {
                    _apparelProDbContext.StyleComponentBreakdowns.RemoveRange(existingRecords);
                }

                if (records != null && records.Count > 0)
                {
                    var dbModels = _mapper.Map<List<StyleComponentBreakdown>>(records);
                    await _apparelProDbContext.StyleComponentBreakdowns.AddRangeAsync(dbModels);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
