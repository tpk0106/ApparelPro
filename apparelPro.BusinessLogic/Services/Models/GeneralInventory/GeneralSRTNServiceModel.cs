using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSRTNServiceModel
    {
        [Required]
        public GeneralSrtnHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralSrtnLineItemServiceModel> Lines { get; set; } = new();
    }
}
