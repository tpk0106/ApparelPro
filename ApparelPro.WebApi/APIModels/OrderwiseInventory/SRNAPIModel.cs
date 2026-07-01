using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SRNAPIModel
    {
        [Required]
        public RequisitionHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<RequisitionLineItemAPIModel> Lines { get; set; } = new();
    }
}
