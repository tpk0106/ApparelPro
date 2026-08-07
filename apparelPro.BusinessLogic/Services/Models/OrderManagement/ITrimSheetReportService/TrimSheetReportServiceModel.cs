using System;
using System.Collections.Generic;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.ITrimSheetReportService
{
    // Replicates OD_TRIM.PRG's "TRIM SHEET" report. Scoped to a single Buyer/Order/Type/Style,
    // matching the legacy screen exactly (single style entry, not a bulk/list report).
    //
    // SCOPE NOTE (2026-08-07, per explicit project decision): legacy's Trim Sheet also totals
    // Sub Contract costs (od_subc) and Production Line costs (pr_lnal/od_line) into GrandTotal.
    // Neither has been migrated to this system yet (no entity, no service, no screen for
    // either), so this report currently covers Material Consumption costs only - GrandTotal
    // below is a material-only total, not the full legacy total. SubContractSectionAvailable
    // and ProductionLineSectionAvailable are both hardcoded false today so the frontend can
    // render an honest "not yet available" placeholder instead of a silently incomplete total.
    public class TrimSheetReportServiceModel
    {
        public int BuyerCode { get; set; }
        // FIXED (2026-08-07): the report header only ever carried BuyerCode/TypeCode -
        // no display name for either, so the frontend had nothing to print but raw codes.
        // Resolved the same way BasisDescription already was below (a lookup by code).
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        // Matches GarmentTypeServiceModel/GarmentTypeAPIModel's existing "TypeName" field name.
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;

        public string Unit { get; set; } = "";
        public decimal StyleQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string BasisCode { get; set; } = "";
        public string BasisDescription { get; set; } = "";
        // The order's currency (od_po->curr / m_curr in legacy) - every line's own currency is
        // converted into this one before any value/subtotal/total is computed.
        public string CurrencyCode { get; set; } = "";

        public List<TrimSheetLineServiceModel> Lines { get; set; } = new();
        public List<TrimSheetStockGroupServiceModel> StockGroupSubtotals { get; set; } = new();
        public List<TrimSheetSupplierTotalServiceModel> SupplierTotals { get; set; } = new();

        // Material-only total today - see the class-level SCOPE NOTE above.
        public decimal GrandTotalValue { get; set; }

        public bool SubContractSectionAvailable { get; set; } = false;
        public bool ProductionLineSectionAvailable { get; set; } = false;

        // Null when the requesting user isn't in one of the profit-visibility roles (mirrors
        // legacy's access('trimprof') gate) - the controller decides whether to compute this at
        // all, rather than the frontend hiding a field it was actually sent.
        public TrimSheetProfitServiceModel? Profit { get; set; }

        // Null when the style has not yet been Trim Sheet approved.
        public TrimSheetApprovalStampServiceModel? ApprovalStamp { get; set; }
    }

    public class TrimSheetLineServiceModel
    {
        public string StockCode { get; set; } = "";
        public string StockDescription { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";

        // When false, legacy prints "** Ignored **" instead of the per-garment consumption
        // figure - this line's TotalConsumption was entered directly rather than derived from
        // QuantityPerGarment (matches StyleMaterialConsumptionLedger.CalculateConsumption / cons_cal).
        public bool IsConsumptionCalculated { get; set; }
        public decimal QuantityPerGarment { get; set; }
        public string ConsumptionUnit { get; set; } = "";

        public decimal TotalConsumption { get; set; }
        public string ItemUnit { get; set; } = "";

        // Unit price converted into the report's CurrencyCode (od_sacc2->price, run through
        // curconv() in legacy / ConvertAsync here).
        public decimal ConvertedUnitPrice { get; set; }
        // TotalConsumption * ConvertedUnitPrice.
        public decimal Value { get; set; }

        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
    }

    // One per distinct StockCode, in the same order the lines first appear (matches legacy's
    // group-as-you-go printing rather than a separate sort pass).
    public class TrimSheetStockGroupServiceModel
    {
        public string StockCode { get; set; } = "";
        public string StockDescription { get; set; } = "";
        public decimal SubtotalValue { get; set; }
        public decimal CostPerGarment { get; set; }
        public decimal PercentageOfUnitPrice { get; set; }
    }

    public class TrimSheetSupplierTotalServiceModel
    {
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public decimal TotalValue { get; set; }
    }

    public class TrimSheetProfitServiceModel
    {
        public decimal UnitPricePerGarment { get; set; }
        public decimal CostPerGarment { get; set; }
        public decimal CostPercentageOfUnitPrice { get; set; }
        public decimal EstimatedProfitPerGarment { get; set; }
        public decimal EstimatedProfitPercentage { get; set; }
    }

    public class TrimSheetApprovalStampServiceModel
    {
        public string ApprovedByUserId { get; set; } = "";
        public DateOnly ApprovedDate { get; set; }
    }
}
