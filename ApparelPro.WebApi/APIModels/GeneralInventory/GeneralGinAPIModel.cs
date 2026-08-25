using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinAPIModel
    {
        [Required]
        public GeneralGinHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralGinLineItemAPIModel> Lines { get; set; } = new();

        // True once the user has seen and confirmed the "minimum stock reached" warning.
        // This alone is NOT sufficient to bypass the check - the controller also requires
        // the authenticated user to hold manager-override authority.
        public bool OverrideMinStockCheck { get; set; } = false;
    }
}
