using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGRNServiceModel
    {
        [Required]
        public GeneralGrnHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralGrnLineItemServiceModel> Lines { get; set; } = new();
    }
}
