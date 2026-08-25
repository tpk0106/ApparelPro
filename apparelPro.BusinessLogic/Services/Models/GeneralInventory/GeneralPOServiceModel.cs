using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPOServiceModel
    {
        [Required]
        public GeneralPoHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralPoLineItemServiceModel> Lines { get; set; } = new();
    }
}
