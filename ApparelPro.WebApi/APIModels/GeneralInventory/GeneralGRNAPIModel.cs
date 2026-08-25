using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGRNAPIModel
    {
        [Required]
        public GeneralGrnHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralGrnLineItemAPIModel> Lines { get; set; } = new();

        // True once the user has seen and confirmed the "exceeds maximum stock" warning.
        public bool MaxStockOverrideConfirmed { get; set; } = false;
    }
}
