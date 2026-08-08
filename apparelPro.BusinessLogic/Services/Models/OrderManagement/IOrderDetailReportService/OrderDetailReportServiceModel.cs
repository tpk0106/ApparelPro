using System;
using System.Collections.Generic;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderDetailReportService
{
    // Replicates OD_RPO1.PRG's "ORDER CONFIRMATION REPORT" print program (legacy menu item
    // "Reports -> B. Order Confirmation Report"), scoped to the "Buyer+Order given, Type
    // blank" case only (legacy's prn_for1 branch) - per explicit project decision
    // (2026-08-07): one full detail printout per Buyer+Order, listing every Style under it
    // expanded into its Part-Shipment lines. The other legacy case (Type also given,
    // legacy's prn_for2 branch - a rolled-up per-style listing with no part-shipment detail)
    // and the bulk "list every order" case (all filters blank) are NOT covered here.
    //
    // SCOPE NOTE (2026-08-07, per explicit project decision - Zero-Assumption gaps found
    // and surfaced before writing any code):
    //  - Legacy's od_po.desc (order description, printed on the header line) has no
    //    equivalent column on this system's PurchaseOrder entity - it was never migrated
    //    anywhere in this codebase. Omitted from this report rather than guessed; adding it
    //    would mean a new PurchaseOrder column plus a capture point in Order Confirmation
    //    Routine, which is a separate feature, not part of this report.
    //  - Legacy's od_part.dest seeks into od_dest (a proper destination master) to resolve a
    //    description. This system's Part Shipment screen (PartShipmentServiceModel /
    //    part-shipments-grid.tsx) stores DestinationCode as free text with no link to the
    //    Destination reference table anywhere in the app today - confirmed by grep, not
    //    assumed. Printed here as the raw code exactly as entered, not a resolved name.
    public class OrderDetailReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public DateOnly OrderDate { get; set; }
        public string Unit { get; set; } = "";
        public string CurrencyCode { get; set; } = "";

        public List<OrderDetailStyleServiceModel> Styles { get; set; } = new();

        // Sum of every part-shipment line's Value across every Style in this order.
        public decimal GrandTotalValue { get; set; }
    }

    public class OrderDetailStyleServiceModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";

        public string Unit { get; set; } = "";
        // The style's total order quantity (od_style.qty) - not the same as TotalQuantity
        // below, which is the sum actually scheduled across this style's part shipments.
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public List<OrderDetailPartShipmentServiceModel> PartShipments { get; set; } = new();

        // Sums across PartShipments - 0 for a style with no part shipments scheduled yet.
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class OrderDetailPartShipmentServiceModel
    {
        // Split-shipment export order reference (od_part.new_order).
        public string NewOrder { get; set; } = "";
        // Raw destination code as entered - see the class-level SCOPE NOTE above.
        public string DestinationCode { get; set; } = "";
        public DateOnly ShipDate { get; set; }

        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        // The owning Style's UnitPrice * this line's Quantity.
        public decimal Value { get; set; }
    }
}
