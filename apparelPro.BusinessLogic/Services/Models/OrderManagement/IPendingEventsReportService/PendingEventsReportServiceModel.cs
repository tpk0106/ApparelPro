namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPendingEventsReportService
{
    // Replicates OD_EVPND.PRG's "PENDING EVENTS" report - lists every EventMaster
    // (od_emast) row not yet actioned (ActualDate/rcv_date blank) whose ScheduledDate
    // (exp_date) is either unset or already due as of the given As Of Date, grouped by
    // Buyer/Order/Type/Style the same way the legacy print breaks on key change.
    public class PendingEventRowServiceModel
    {
        public string EventCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public DateTime? ScheduledDate { get; set; }
        public string? Remarks { get; set; }
        // Null when ScheduledDate is null (legacy prints "No Scheduled Date" in that
        // case instead of a day count) - matches "m_date - c_tod(exp_date)".
        public int? DelayDays { get; set; }
    }

    public class PendingEventStyleGroupServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public List<PendingEventRowServiceModel> Events { get; set; } = new();
    }

    public class PendingEventsReportServiceModel
    {
        public DateTime AsOfDate { get; set; }
        public List<PendingEventStyleGroupServiceModel> Groups { get; set; } = new();
    }
}
