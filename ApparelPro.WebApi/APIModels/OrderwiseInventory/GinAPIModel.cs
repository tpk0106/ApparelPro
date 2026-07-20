using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinAPIModel
    {
        [Required]
        public GinHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GinLineItemAPIModel> Lines { get; set; } = new();

        // True once the user has seen and confirmed the "issue below exact consumption"
        // warning. This alone is NOT sufficient to bypass the check — the controller also
        // requires the authenticated user to hold manager-override authority. A client
        // cannot self-grant override rights by simply flipping this flag.
        public bool OverrideExactConsumptionCheck { get; set; } = false;
    }
}
