using System.Collections.Generic;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService
{
    // Full material catalog grouped by Stock category, for the material-master-list
    // catalog picker. Always includes every Stock category, even ones with no
    // OrderItems cataloged yet, so the UI can show an empty-but-visible category.
    public class MaterialCatalogGroupServiceModel
    {
        public string StockCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<MaterialCatalogItemServiceModel> Items { get; set; } = new();
    }
}
