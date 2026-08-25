using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGTNServiceModel
    {
        [Required]
        public GeneralGtnHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralGtnLineItemServiceModel> Lines { get; set; } = new();
    }
}
