using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralRTNServiceModel
    {
        [Required]
        public GeneralRtnHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralRtnLineItemServiceModel> Lines { get; set; } = new();
    }
}
