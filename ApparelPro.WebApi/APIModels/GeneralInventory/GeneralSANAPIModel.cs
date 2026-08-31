using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSANAPIModel
    {
        [Required]
        public GeneralSanHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralSanLineItemAPIModel> Lines { get; set; } = new();
    }
}
