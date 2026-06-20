using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class SaveSupplierPORequestAPIModel
    {
        [Required] public POHeaderAPIModel Header { get; set; } = null!;
        [Required] public List<PODetailLineAPIModel> LineItems { get; set; } = new();

    }
}
