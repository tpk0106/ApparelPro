using System.Collections.Generic;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class MaterialCatalogGroupAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<MaterialCatalogItemAPIModel> Items { get; set; } = new();
    }
}
