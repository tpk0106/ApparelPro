using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralDGNAPIModel
    {
        [Required]
        public GeneralDgnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralDgnLineItemAPIModel> Lines { get; set; } = new();
    }
}
