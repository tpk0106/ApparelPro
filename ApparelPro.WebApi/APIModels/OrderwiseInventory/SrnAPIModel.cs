using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SrnAPIModel
    {
        [Required]
        public SrnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<SrnLineItemAPIModel> Lines { get; set; } = new();
    }
}
