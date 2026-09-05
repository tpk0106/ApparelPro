using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DtnAPIModel
    {
        [Required]
        public DtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<DtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
