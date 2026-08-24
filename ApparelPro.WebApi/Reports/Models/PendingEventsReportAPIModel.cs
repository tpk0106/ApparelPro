namespace ApparelPro.WebApi.Reports.Models
{
    public class PendingEventRowAPIModel
    {
        public string EventCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public DateTime? ScheduledDate { get; set; }
        public string? Remarks { get; set; }
        public int? DelayDays { get; set; }
    }

    public class PendingEventStyleGroupAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public List<PendingEventRowAPIModel> Events { get; set; } = new();
    }

    public class PendingEventsReportAPIModel
    {
        public DateTime AsOfDate { get; set; }
        public List<PendingEventStyleGroupAPIModel> Groups { get; set; } = new();
    }
}
