using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    // Replicates od_tpdtr.dbf - "Type/Accessory Breakdown" / "Garment Type wise Item
    // Requirements" - Reference Files > B. Order Management > G. Type / Item in legacy
    // (RF_MENU.PRG, OD_ITM1.PRG/OD_ITM2.PRG). For each Garment Type, a default list of
    // items/accessories (Stock Code + Item Code) with a default Unit + Quantity.
    public class GarmentTypeItems
    {
        public int Id { get; set; }

        public int GarmentTypeId { get; set; }

        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }

        [ForeignKey("GarmentTypeId")]
        public virtual GarmentType GarmentType { get; set; } = null!;
    }
}
