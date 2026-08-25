using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralRTNAPIModel
    {
        [Required]
        public GeneralRtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralRtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
