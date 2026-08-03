using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService
{
    public class AvailableBudgetLineServiceModel
    {
        // Holds the unified 22+ character composite string key (StockCode + ItemCode + Features)
        public string ItemCode { get; set; } = null!;
        public string ItemUnit { get; set; } = null!;
        public decimal BalanceQuantity { get; set; }
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Informational only - the real, enforced gate is the server-side check inside
        // SaveSupplierPurchaseOrderAsync (mirrors the existing budget-deficit check in that
        // same method: client-facing flag for a good UX, hard guard is server-side). True when
        // this line's Style has completed Trim Sheet Approval (legacy: od_style->userid not
        // empty - see OD_ACORD.PRG's rajiva() function) and can actually be pulled into a P/O.
        public bool IsStyleApproved { get; set; }

        // The SPECIFIC material type (e.g. "BUTTON", "FABRIC", "ZIPPER") - resolved from
        // the OrderItems catalog, keyed by (StockCode, ItemCode) segments of the composite
        // ItemCode. Falls back to the broader Stock category description (e.g. "Raw material")
        // only if no OrderItems catalog entry exists. Shown next to the raw ItemCode in the
        // picker grid so operators don't have to decode the composite code by eye.
        public string MainMaterialName { get; set; } = string.Empty;
    }
}
