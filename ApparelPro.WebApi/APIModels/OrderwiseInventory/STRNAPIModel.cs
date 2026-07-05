using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class STRNAPIModel
    {
        [Required]
        public RequisitionHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<RequisitionLineItemAPIModel> Lines { get; set; } = new();
    }
}
