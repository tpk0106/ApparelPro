using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnAPIModel
    {
        [Required]
        public ArnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<ArnLineItemAPIModel> Lines { get; set; } = new();
    }
}
