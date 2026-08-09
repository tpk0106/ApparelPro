using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class AinAPIModel
    {
        [Required]
        public AinHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<AinLineItemAPIModel> Lines { get; set; } = new();
    }
}
