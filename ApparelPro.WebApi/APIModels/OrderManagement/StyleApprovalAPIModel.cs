using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class StyleApprovalAPIModel
    {
        [Required] public int BuyerCode { get; set; }
        [Required] public string Order { get; set; } = null!;
        [Required] public int TypeCode { get; set; }
        [Required] public string StyleCode { get; set; } = null!;
        [Required] public string ApprovedByUserId { get; set; } = null!;
        [Required] public DateTime ApprovalDate { get; set; }
    }

}
