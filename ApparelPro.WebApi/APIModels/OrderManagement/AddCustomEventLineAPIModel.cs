using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class AddCustomEventLineAPIModel
    {
        [Required] public int BuyerCode { get; set; }
        [Required] public string Order { get; set; } = null!;
        [Required] public int TypeCode { get; set; }
        [Required] public string StyleCode { get; set; } = null!;
        [Required] public string EventCode { get; set; } = null!;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public string? Remarks { get; set; }
    }
}
