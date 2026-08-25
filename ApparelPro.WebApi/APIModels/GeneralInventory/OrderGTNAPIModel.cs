using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class OrderGTNAPIModel
    {
        [Required]
        public OrderGtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<OrderGtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
