using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class RtnAPIModel
    {
        [Required]
        public RtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<RtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
