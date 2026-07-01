using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStylewiseEvents
{
    public class StylewiseEventServiceModel
    {
        public int Id { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string EventCode { get; set; } = null!;
        public string Description { get; set; } = null!; // Fetched from your master lookup reference
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public string? Remarks { get; set; }
        public string MilestoneStatus { get; set; } = null!; // Dynamic calculation: "No Scheduled Date", etc.

        // ADD THESE TWO COLUMNS TO YOUR FLAT ROW MODEL:
        public string? ApprovedByUserId { get; set; } // ea_userid
        public DateTime? ApprovedDate { get; set; }    // ea_date
    }
}
