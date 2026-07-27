using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GtnAPIModel
    {
        [Required]
        public GtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
