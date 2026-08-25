using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class OrderGTNServiceModel
    {
        [Required]
        public OrderGtnHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<OrderGtnLineItemServiceModel> Lines { get; set; } = new();
    }
}
