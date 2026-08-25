using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSTRNAPIModel
    {
        [Required]
        public GeneralRequisitionHeaderAPIModel Header { get; set; } = null!;

        [Required]
        public List<GeneralRequisitionLineItemAPIModel> Lines { get; set; } = new();
    }
}
