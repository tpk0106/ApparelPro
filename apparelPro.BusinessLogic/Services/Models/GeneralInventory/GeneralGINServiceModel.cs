using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGINServiceModel
    {
        [Required]
        public GeneralGinHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralGinLineItemServiceModel> Lines { get; set; } = new();
    }
}
