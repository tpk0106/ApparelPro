using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSRTNAPIModel
    {
        [Required]
        public GeneralSrtnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralSrtnLineItemAPIModel> Lines { get; set; } = new();
    }
}
