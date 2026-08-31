using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSANServiceModel
    {
        [Required]
        public GeneralSanHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralSanLineItemServiceModel> Lines { get; set; } = new();
    }
}
