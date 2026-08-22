using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.OrderManagement
{
    public class PurchaseOrder
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public DateTime OrderDate { get; set; }
        public int GarmentType { get; set; }
        [NotMapped]
        public string GarmentTypeName { get; set; }

        // Legacy OD_PO.DESC - free-text order description entered at PO
        // creation (e.g. "Mens winter jacket"). Never migrated onto this
        // entity until now; existing rows will be null until re-saved.
        public string? Description { get; set; }
        [NotMapped]
        public string? Buyer { get; set; }
        public string CountryCode { get; set; }
        public string UnitCode { get; set; }
        public decimal TotalQuantity { get; set; }
        public string CurrencyCode { get; set; }
        public string Season { get; set; }
        public string BasisCode { get; set; }
        public decimal BasisValue { get; set; }

        // Audit trail for the 'AllowOrderQuantityOverride' system parameter: set only when a
        // Style Details save is let through despite pushing the running style total past
        // TotalQuantity, so it's always clear who authorized it and when. Null under normal
        // (never-overridden) operation.
        public string? QuantityOverriddenBy { get; set; }
        public DateTime? QuantityOverriddenAt { get; set; }
    }
}
