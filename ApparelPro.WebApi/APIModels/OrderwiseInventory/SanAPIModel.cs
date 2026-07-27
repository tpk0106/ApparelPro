using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SanAPIModel
    {
        [Required]
        public SanHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<SanLineItemAPIModel> Lines { get; set; } = new();
    }
}
