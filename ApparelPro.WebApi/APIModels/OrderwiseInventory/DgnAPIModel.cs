using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DgnAPIModel
    {
        [Required]
        public DgnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<DgnLineItemAPIModel> Lines { get; set; } = new();
    }
}
