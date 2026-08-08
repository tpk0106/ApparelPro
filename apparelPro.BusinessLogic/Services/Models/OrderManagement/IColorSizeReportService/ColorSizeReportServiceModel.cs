using System;
using System.Collections.Generic;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeReportService
{
    // Replicates OD_CLSZ3.PRG's "COLOUR / SIZE DETAILS" print program (legacy menu item
    // "Reports -> C. Colour/Size Report"), scoped exactly as legacy is - Buyer+Order,
    // every Style, every Colour, pivoted by Size.
    //
    // SCOPE NOTE (2026-08-08): legacy's od_clqr (buyer/order/type/style/color/desc/
    // quantity) is not a dataset registered in DATAMODELS.md. It is NOT treated as a
    // missing table here - every field it prints is derivable from ColorSizeDetails
    // (the already-established modern equivalent of legacy's od_cszdt, confirmed by
    // the 2026-08-07 Description fix touching this exact table) by grouping on
    // Style+Colour: od_clqr's "desc" is ColorSizeDetails.Description, and its
    // "quantity" is SUM(ColorSizeDetails.Qty) for that Style+Colour. Nothing od_clqr
    // prints is independent data, so no new entity/migration is needed for this report.
    public class ColorSizeReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;

        // Distinct Sizes across every Style/Colour in this Buyer+Order, sorted with an
        // ordinal string comparison - mirrors OD_CLSZ3.PRG's `inde on size to &user_no`
        // unique index (Clipper indexes sort by raw character value, not a garment-size-
        // aware order like S/M/L/XL). Used as the fixed column set for every Style block,
        // even where a given Style/Colour has no row for one.
        public List<string> SizeColumns { get; set; } = new();

        public List<ColorSizeReportStyleServiceModel> Styles { get; set; } = new();
    }

    public class ColorSizeReportStyleServiceModel
    {
        public string StyleCode { get; set; } = null!;
        public List<ColorSizeReportColourServiceModel> Colours { get; set; } = new();

        // Per-size column sum across every Colour in this Style, plus the style's
        // overall total - mirrors the "Total" row OD_CLSZ3.PRG prints under each
        // Style block.
        public Dictionary<string, decimal> SizeTotals { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    public class ColorSizeReportColourServiceModel
    {
        public string ColorCode { get; set; } = null!;
        public string Description { get; set; } = "";

        // Keyed by Size code (matches ColorSizeReportServiceModel.SizeColumns) - a Size
        // with no Colour/Size Details row for this Colour is simply absent from the
        // dictionary (renders as 0 on the frontend/PDF), not fabricated as an explicit
        // zero-value row.
        public Dictionary<string, decimal> SizeQuantities { get; set; } = new();
        public decimal TotalQuantity { get; set; }
    }
}
