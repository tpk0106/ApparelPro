using System.ComponentModel.DataAnnotations;

namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSTRNServiceModel
    {
        [Required]
        public GeneralRequisitionHeaderServiceModel Header { get; set; } = null!;

        [Required]
        public List<GeneralRequisitionLineItemServiceModel> Lines { get; set; } = new();
    }
}
