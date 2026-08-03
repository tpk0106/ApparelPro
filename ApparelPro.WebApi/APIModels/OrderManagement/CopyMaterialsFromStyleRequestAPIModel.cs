using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CopyMaterialsFromStyleRequestAPIModel
    {
        [Required] public int SourceBuyerCode { get; set; }
        [Required] public string SourceOrder { get; set; } = null!;
        [Required] public int SourceTypeCode { get; set; }
        [Required] public string SourceStyleCode { get; set; } = null!;

        [Required] public int TargetBuyerCode { get; set; }
        [Required] public string TargetOrder { get; set; } = null!;
        [Required] public int TargetTypeCode { get; set; }
        [Required] public string TargetStyleCode { get; set; } = null!;
    }
}
