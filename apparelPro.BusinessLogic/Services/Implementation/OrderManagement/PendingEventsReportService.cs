using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPendingEventsReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_EVPND.PRG's "PENDING EVENTS" report.
    public class PendingEventsReportService : IPendingEventsReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public PendingEventsReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PendingEventsReportServiceModel> GetPendingEventsReportAsync(DateTime asOfDate)
        {
            var pendingEvents = await _apparelProDbContext.EventMasters
                .AsNoTracking()
                .Where(e => e.ActualDate == null && (e.ScheduledDate == null || e.ScheduledDate <= asOfDate))
                .OrderBy(e => e.BuyerCode).ThenBy(e => e.Order).ThenBy(e => e.TypeCode).ThenBy(e => e.StyleCode)
                .ThenBy(e => e.ScheduledDate)
                .ToListAsync();

            // Mirrors legacy's implicit "nothing to print" outcome (no explicit error box
            // in OD_EVPND.PRG itself, but every other Order Management report in this
            // project surfaces one for an empty result - same convention as Scheduled
            // Shipments Report).
            if (pendingEvents.Count == 0)
                throw new InvalidOperationException("No Pending Events For Printing.");

            var eventCodes = pendingEvents.Select(e => e.EventCode).Distinct().ToList();
            var eventDescriptions = await _apparelProDbContext.StylewiseEvents
                .AsNoTracking()
                .Where(ev => eventCodes.Contains(ev.EventCode))
                .ToDictionaryAsync(ev => ev.EventCode, ev => ev.Description);

            // Buyer/Type are always shown by name in this project's reports, never by raw
            // code - same convention as Year/Season Wise Orders and Order Detail Report.
            var buyerCodes = pendingEvents.Select(e => e.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var typeCodes = pendingEvents.Select(e => e.TypeCode).Distinct().ToList();
            var typeNames = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => typeCodes.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.TypeName);

            var groups = pendingEvents
                .GroupBy(e => (e.BuyerCode, e.Order, e.TypeCode, e.StyleCode))
                .Select(g => new PendingEventStyleGroupServiceModel
                {
                    BuyerCode = g.Key.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(g.Key.BuyerCode, g.Key.BuyerCode.ToString()),
                    Order = g.Key.Order,
                    TypeCode = g.Key.TypeCode,
                    TypeName = typeNames.GetValueOrDefault(g.Key.TypeCode, ""),
                    StyleCode = g.Key.StyleCode,
                    Events = g.Select(e => new PendingEventRowServiceModel
                    {
                        EventCode = e.EventCode,
                        Description = eventDescriptions.GetValueOrDefault(e.EventCode, ""),
                        ScheduledDate = e.ScheduledDate,
                        Remarks = e.Remarks,
                        DelayDays = e.ScheduledDate.HasValue ? (asOfDate.Date - e.ScheduledDate.Value.Date).Days : null,
                    }).ToList(),
                })
                .ToList();

            return new PendingEventsReportServiceModel
            {
                AsOfDate = asOfDate,
                Groups = groups,
            };
        }
    }
}
