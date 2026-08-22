using apparelPro.BusinessLogic.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionTimeTicketService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    // SRP: owns ticket persistence + orchestrates the efficiency summary;
    // the summary math itself lives entirely in
    // DailyProductionEfficiencyCalculator (DIP - pure, no DB dependency).
    public class DailyProductionTimeTicketService : IDailyProductionTimeTicketService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public DailyProductionTimeTicketService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<DailyProductionTimeTicketServiceModel> GetByTicketAsync(
            DateOnly date, string lineCode, int buyerCode, string order, int typeCode, string styleCode)
        {
            var entities = await _apparelProDbContext.DailyProductionTimeTicketEntries
                .AsNoTracking()
                .Where(e => e.Date == date && e.LineCode == lineCode &&
                            e.BuyerCode == buyerCode && e.Order == order &&
                            e.TypeCode == typeCode && e.StyleCode == styleCode)
                .ToListAsync();

            var summaries = await ComputeSummariesAsync(buyerCode, order, typeCode, styleCode, entities);

            return new DailyProductionTimeTicketServiceModel
            {
                Entries = _mapper.Map<List<DailyProductionTimeTicketEntryServiceModel>>(entities),
                EmployeeSummaries = summaries
            };
        }

        public async Task<DailyProductionTimeTicketServiceModel> BulkSaveAsync(
            DateOnly date, string lineCode, int buyerCode, string order, int typeCode, string styleCode,
            List<CreateDailyProductionTimeTicketEntryServiceModel> records)
        {
            // Matches legacy's "Not a valid Operation Code for given Style"
            // check (PR_DPTT1.PRG line 472-475): every entry's operation must
            // exist in this style's Operation Breakdown before it can be saved.
            var validOperationCodes = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode && b.Order == order &&
                            b.TypeCode == typeCode && b.StyleCode == styleCode)
                .Select(b => b.OperationCode)
                .Distinct()
                .ToListAsync();

            var invalidCodes = records
                .Select(r => r.OperationCode)
                .Distinct()
                .Except(validOperationCodes)
                .ToList();

            if (invalidCodes.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Not a valid Operation Code for given Style: {string.Join(", ", invalidCodes)}");
            }

            using var dbTransaction = await _apparelProDbContext.Database
                .BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
            try
            {
                var existingEntries = await _apparelProDbContext.DailyProductionTimeTicketEntries
                    .Where(e => e.Date == date && e.LineCode == lineCode &&
                                e.BuyerCode == buyerCode && e.Order == order &&
                                e.TypeCode == typeCode && e.StyleCode == styleCode)
                    .ToListAsync();

                if (existingEntries.Count > 0)
                {
                    _apparelProDbContext.DailyProductionTimeTicketEntries.RemoveRange(existingEntries);
                }

                var newEntities = records.Select(r => new DailyProductionTimeTicketEntry
                {
                    Date = date,
                    LineCode = lineCode,
                    BuyerCode = buyerCode,
                    Order = order,
                    TypeCode = typeCode,
                    StyleCode = styleCode,
                    EmployeeCode = r.EmployeeCode,
                    OperationCode = r.OperationCode,
                    Quantity = r.Quantity,
                    NonProductiveHourCode = r.NonProductiveHourCode,
                    NonProductiveHours = r.NonProductiveHours,
                    WorkHours = r.WorkHours
                }).ToList();

                if (newEntities.Count > 0)
                {
                    await _apparelProDbContext.DailyProductionTimeTicketEntries.AddRangeAsync(newEntities);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                var summaries = await ComputeSummariesAsync(buyerCode, order, typeCode, styleCode, newEntities);

                return new DailyProductionTimeTicketServiceModel
                {
                    Entries = _mapper.Map<List<DailyProductionTimeTicketEntryServiceModel>>(newEntities),
                    EmployeeSummaries = summaries
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<EmployeeEfficiencySummaryServiceModel>> ComputeSummariesAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            List<DailyProductionTimeTicketEntry> entries)
        {
            if (entries.Count == 0) return new List<EmployeeEfficiencySummaryServiceModel>();

            var samByOperation = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode && b.Order == order &&
                            b.TypeCode == typeCode && b.StyleCode == styleCode)
                .GroupBy(b => b.OperationCode)
                .Select(g => new { OperationCode = g.Key, Sam = g.First().Sam })
                .ToDictionaryAsync(x => x.OperationCode, x => x.Sam);

            var calculatorInputs = entries.Select(e => new ProductionEntryInput
            {
                EmployeeCode = e.EmployeeCode,
                Quantity = e.Quantity,
                Sam = samByOperation.TryGetValue(e.OperationCode, out var sam) ? sam : 0,
                NonProductiveHours = e.NonProductiveHours,
                WorkHours = e.WorkHours
            }).ToList();

            var summaries = DailyProductionEfficiencyCalculator.Summarize(calculatorInputs);

            return summaries.Select(s => new EmployeeEfficiencySummaryServiceModel
            {
                EmployeeCode = s.EmployeeCode,
                WorkHours = s.WorkHours,
                NonProductiveHours = s.NonProductiveHours,
                EarnedMinutes = s.EarnedMinutes,
                OverEfficiencyPercent = s.OverEfficiencyPercent,
                OperatorEfficiencyPercent = s.OperatorEfficiencyPercent
            }).ToList();
        }
    }
}
