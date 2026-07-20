using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnAPIModel
    {
        [Required]
        public GrnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GrnLineItemAPIModel> Lines { get; set; } = new();
    }
}
