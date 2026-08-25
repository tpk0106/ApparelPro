using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPOAPIModel
    {
        [Required]
        public GeneralPoHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralPoLineItemAPIModel> Lines { get; set; } = new();
    }
}
