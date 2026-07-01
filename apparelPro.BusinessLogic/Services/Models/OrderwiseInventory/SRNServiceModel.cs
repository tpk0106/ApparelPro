using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SRNServiceModel
    {
        [Required]
        public RequisitionHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<RequisitionLineItemServiceModel> Lines { get; set; } = new();

    }
}
