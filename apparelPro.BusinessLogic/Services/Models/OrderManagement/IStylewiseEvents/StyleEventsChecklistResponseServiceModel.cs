using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStylewiseEvents
{
    public class StyleEventsChecklistResponseServiceModel
    {
        public bool IsApproved => !string.IsNullOrEmpty(ApprovedByUserId);
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedDate { get; set; } = DateTime.MinValue;
        public List<StylewiseEventServiceModel> Items { get; set; } = new();

    }
}
