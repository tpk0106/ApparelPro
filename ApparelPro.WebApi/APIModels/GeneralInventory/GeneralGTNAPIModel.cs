using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGTNAPIModel
    {
        [Required]
        public GeneralGtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralGtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
