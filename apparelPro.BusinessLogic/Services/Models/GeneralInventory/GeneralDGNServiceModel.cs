using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralDGNServiceModel
    {
        [Required]
        public GeneralDgnHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralDgnLineItemServiceModel> Lines { get; set; } = new();
    }
}
