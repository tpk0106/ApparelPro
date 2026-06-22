using ApparelPro.WebApi.APIModels.OrderManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService
{
    public class SaveSupplierPORequestServiceModel
    {
        [Required] public POHeaderServiceModel Header { get; set; } = null!;
        [Required] public List<PODetailLineServiceModel> LineItems { get; set; } = new();
    }
}
