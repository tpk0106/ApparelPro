using System.ComponentModel.DataAnnotations.Schema;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events
{
    public class EventMaster
    {
        public int Id { get; set; } // Internal database auto-increment identity seed

        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;

        // Links straight to your master milestones dictionary string code
        public string EventCode { get; set; } = null!; // ev_code

        public DateTime? ScheduledDate { get; set; } // exp_date
        public DateTime? ActualDate { get; set; }    // rcv_date
        public string? Remarks { get; set; }         // remarks

        // Fluent navigation property linking back to your master milestones dictionary shape
        [ForeignKey("EventCode")]
        public virtual StylewiseEvent MilestoneEvent { get; set; } = null!;
    }
}
