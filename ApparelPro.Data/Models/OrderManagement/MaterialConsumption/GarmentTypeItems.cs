using ApparelPro.Data.Models.References;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class GarmentTypeItems
    {
        // Replicates the true od_tpdtr.dbf schema architecture
        
            public int Id { get; set; }

            // Binds directly to the primary key Id of your independent GarmentType reference file
            public int GarmentTypeId { get; set; }

            public string StockCode { get; set; } = null!;
            public string ItemCode { get; set; } = null!;
            public string Unit { get; set; } = null!;
            public decimal Quantity { get; set; }

            // Optional: Fluent navigation properties to link the models inside C#
            [ForeignKey("GarmentTypeId")]
            public virtual GarmentType GarmentType { get; set; } = null!;
        
    }
}
