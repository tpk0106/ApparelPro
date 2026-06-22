using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStylewiseEvents;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events;
using ApparelPro.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class StylewiseEventService : IStylewiseEventService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IMapper _mapper;

        public StylewiseEventService(ApparelProDbContext apparelProDbContext, IMapper mapper)
        {
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
        }

        public async Task<List<StylewiseEventServiceModel>> GetStyleEventsAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            // 1. Check if this Style already has milestones logged inside the EventMasters table
            var hasExistingEntries = await _apparelProDbContext.EventMasters
                .AnyAsync(e => e.BuyerCode == buyerCode && e.Order == order && e.TypeCode == typeCode && e.StyleCode == styleCode);

            // 2. CLIPPER AUTOMATED POPULATION: If brand new, copy all master rows from StylewiseEvents
            if (!hasExistingEntries)
            {
                var globalTemplates = await _apparelProDbContext.StylewiseEvents.AsNoTracking().ToListAsync();

                if (globalTemplates.Any())
                {
                    var initialRows = globalTemplates.Select(t => new EventMaster
                    {
                        BuyerCode = buyerCode,
                        Order = order,
                        TypeCode = typeCode,
                        StyleCode = styleCode,
                        EventCode = t.EventCode,
                        ScheduledDate = null,
                        ActualDate = null,
                        Remarks = ""
                    }).ToList();

                    await _apparelProDbContext.EventMasters.AddRangeAsync(initialRows);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }

            // 3. Extract the complete checklist joined to your master descriptions dictionary
            var ledgerRows = await _apparelProDbContext.EventMasters
                .Where(e => e.BuyerCode == buyerCode && e.Order == order && e.TypeCode == typeCode && e.StyleCode == styleCode)
                .ToListAsync();

            var resultList = new List<StylewiseEventServiceModel>();

            foreach (var row in ledgerRows)
            {
                var masterDef = await _apparelProDbContext.StylewiseEvents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.EventCode == row.EventCode);

                // 4. CLIPPER REPORT ENGINE MATRIX: Calculate dynamic milestone status descriptions on the fly
                string calculatedStatus = "** OK **";
                if (!row.ScheduledDate.HasValue) calculatedStatus = "No Scheduled Date";
                else if (!row.ActualDate.HasValue) calculatedStatus = "Actual Date Pending";

                resultList.Add(new StylewiseEventServiceModel
                {
                    Id = row.Id,
                    BuyerCode = row.BuyerCode,
                    Order = row.Order,
                    TypeCode = row.TypeCode,
                    StyleCode = row.StyleCode,
                    EventCode = row.EventCode,
                    Description = masterDef?.Description ?? "Custom Component Milestone",
                    ScheduledDate = row.ScheduledDate,
                    ActualDate = row.ActualDate,
                    Remarks = row.Remarks,
                    MilestoneStatus = calculatedStatus
                });
            }

            return resultList.OrderBy(r => r.EventCode).ToList();
        }

        public async Task<bool> UpdateStyleEventLineAsync(int buyerCode, string order, int typeCode, string styleCode, string eventCode, DateTime? scheduledDate, DateTime? actualDate, string? remarks)
        {
            var record = await _apparelProDbContext.EventMasters
                .FirstOrDefaultAsync(e => e.BuyerCode == buyerCode && e.Order == order.Trim() && e.TypeCode == typeCode && e.StyleCode == styleCode.Trim() && e.EventCode == eventCode.Trim());

            if (record == null) return false;

            // COMPLIANCE LOCK BARRIER: Fetch the Style master row to check for executive approval signatures (ea_userid)
            var styleHeader = await _apparelProDbContext.Styles
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order.Trim() && s.TypeCode == typeCode && s.StyleCode == styleCode.Trim());

            // If executive-approved, skip altering ScheduledDate, but allow updating ActualDate and Remarks!
            bool isApproved = styleHeader != null && !string.IsNullOrEmpty(styleHeader.EstimateApprovalUserName); // Assuming ApprovedByUserId maps to ea_userid

            if (!isApproved)
            {
                record.ScheduledDate = scheduledDate;
            }

            record.ActualDate = actualDate;
            record.Remarks = remarks?.Trim().ToUpper();

            _apparelProDbContext.EventMasters.Update(record);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddCustomStyleEventLineAsync(int buyerCode, string order, int typeCode, string styleCode, string eventCode, DateTime? scheduledDate, DateTime? actualDate, string? remarks)
        {
            eventCode = eventCode.Trim().ToUpper();

            // Check lock barrier before custom manual additions
            var styleHeader = await _apparelProDbContext.Styles
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order.Trim() && s.TypeCode == typeCode && s.StyleCode == styleCode.Trim());

            if (styleHeader != null && !string.IsNullOrEmpty(styleHeader.EstimateApprovalUserName))
            {
                throw new InvalidOperationException("Milestone Aborted: Style Events have been officially approved by management. Custom lines cannot be appended.");
            }

            var exists = await _apparelProDbContext.EventMasters.AnyAsync(e => e.BuyerCode == buyerCode && e.Order == order.Trim() && e.TypeCode == typeCode && e.StyleCode == styleCode.Trim() && e.EventCode == eventCode);
            if (exists) return false;

            var newRow = new EventMaster
            {
                BuyerCode = buyerCode,
                Order = order.Trim(),
                TypeCode = typeCode,
                StyleCode = styleCode.Trim(),
                EventCode = eventCode,
                ScheduledDate = scheduledDate,
                ActualDate = actualDate,
                Remarks = remarks?.Trim().ToUpper()
            };

            await _apparelProDbContext.EventMasters.AddAsync(newRow);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStyleEventLineAsync(int buyerCode, string order, int typeCode, string styleCode, string eventCode)
        {
            var record = await _apparelProDbContext.EventMasters
                .FirstOrDefaultAsync(e => e.BuyerCode == buyerCode && e.Order == order.Trim() && e.TypeCode == typeCode && e.StyleCode == styleCode.Trim() && e.EventCode == eventCode.Trim());

            if (record == null) return true;

            // Check lock barrier before custom manual deletions
            var styleHeader = await _apparelProDbContext.Styles
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order.Trim() && s.TypeCode == typeCode && s.StyleCode == styleCode.Trim());

            if (styleHeader != null && !string.IsNullOrEmpty(styleHeader.EstimateApprovalUserName))
            {
                throw new InvalidOperationException("Milestone Lock: Style Events are approved by management. Row deletion is blocked.");
            }

            _apparelProDbContext.EventMasters.Remove(record);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
