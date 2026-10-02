using apparelPro.BusinessLogic.Services.Models.OrderManagement.ITrimSheetReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.TrimSheet
{
    // Replicates OD_TRIM.PRG's printed "T R I M   S H E E T" layout. See the SCOPE NOTE on
    // TrimSheetReportServiceModel for the Sub Contract / Production Line sections this does
    // not yet render (rendered here as an explicit "not yet available" note instead of being
    // silently missing).
    public static class TrimSheetReportEngine
    {
        public static byte[] GeneratePdf(TrimSheetReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ T R I M   S H E E T ]").FontSize(14).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            // FIXED (2026-08-07): this printed only the raw Buyer/Type codes -
                            // report.BuyerName/TypeName were already being resolved and sent by the
                            // service layer (see TrimSheetReportServiceModel), the PDF engine just
                            // never rendered them. Matches the Basis line's existing "code - description"
                            // fallback pattern below (falls back to the code alone if the name is blank).
                            row.RelativeItem().Text(t => { t.Span("Buyer: ").Bold(); t.Span(string.IsNullOrEmpty(report.BuyerName) ? report.BuyerCode.ToString() : report.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order: ").Bold(); t.Span(report.Order); });
                            row.RelativeItem().Text(t => { t.Span("Type: ").Bold(); t.Span(string.IsNullOrEmpty(report.TypeName) ? report.TypeCode.ToString() : report.TypeName); });
                            row.RelativeItem().Text(t => { t.Span("Style: ").Bold(); t.Span(report.StyleCode); });
                        });
                        column.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Unit: ").Bold(); t.Span(report.Unit); });
                            row.RelativeItem().Text(t => { t.Span("Qty: ").Bold(); t.Span(report.StyleQuantity.ToString("#,##0.##")); });
                            row.RelativeItem().Text(t => { t.Span("Unit Price: ").Bold(); t.Span(report.UnitPrice.ToString("#,##0.00")); });
                            row.RelativeItem().Text(t => { t.Span("Basis: ").Bold(); t.Span(string.IsNullOrEmpty(report.BasisDescription) ? report.BasisCode : $"{report.BasisCode} - {report.BasisDescription}"); });
                        });

                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        // One mini-table per stock group, in first-appearance order, each
                        // followed by its own subtotal line - mirrors OD_TRIM.PRG's per-stock
                        // "Values for {description}" block rather than one giant flat table.
                        var groupedLines = report.Lines.GroupBy(l => l.StockCode);
                        foreach (var group in groupedLines)
                        {
                            var subtotal = report.StockGroupSubtotals.FirstOrDefault(g => g.StockCode == group.Key);

                            column.Item().PaddingTop(8).Text(group.First().StockDescription).Bold().Underline().FontSize(10);

                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.5f); // Item code / description
                                    columns.ConstantColumn(75);   // Consumption per garment
                                    columns.ConstantColumn(80);   // Order qty (total consumption)
                                    columns.ConstantColumn(65);   // Unit price
                                    columns.ConstantColumn(75);   // Value
                                    columns.ConstantColumn(70);   // Supplier
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Item Code / Description").Bold().FontSize(8);
                                    header.Cell().Text("Consump.").Bold().FontSize(8);
                                    header.Cell().Text("Order Qty").Bold().FontSize(8);
                                    header.Cell().Text($"Price ({report.CurrencyCode})").Bold().FontSize(8);
                                    header.Cell().Text($"Value ({report.CurrencyCode})").Bold().FontSize(8);
                                    header.Cell().Text("Supplier").Bold().FontSize(8);
                                    header.Cell().ColumnSpan(6).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                                });

                                foreach (var line in group)
                                {
                                    table.Cell().PaddingVertical(2).Text($"{line.ItemCode} - {line.Description}").FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(
                                        line.IsConsumptionCalculated
                                            ? $"{line.QuantityPerGarment:#,##0.00} {line.ConsumptionUnit}"
                                            : "** Ignored **").FontSize(8);
                                    table.Cell().PaddingVertical(2).Text($"{line.TotalConsumption:#,##0.00} {line.ItemUnit}").FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.ConvertedUnitPrice.ToString("#,##0.000")).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.Value.ToString("#,##0.000")).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.SupplierName).FontSize(8);
                                }
                            });

                            if (subtotal != null)
                            {
                                column.Item().PaddingTop(3).Row(row =>
                                {
                                    row.RelativeItem().Text($"Values for {subtotal.StockDescription} in {report.CurrencyCode} - Cost per Garment: {subtotal.CostPerGarment:#,##0.00} ({subtotal.PercentageOfUnitPrice:0.000}% of Unit Price)")
                                        .FontSize(8).Italic();
                                    row.ConstantItem(100).AlignRight().Text($"Total: {subtotal.SubtotalValue:#,##0.000}").Bold().FontSize(8);
                                });
                            }
                        }

                        // Sub Contract / Production Line - honest placeholder, not a silent gap.
                        if (!report.SubContractSectionAvailable || !report.ProductionLineSectionAvailable)
                        {
                            column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(6).Text(
                                "Sub Contract costs and Production Line costs are not yet available in this system and are excluded from the total below.")
                                .FontSize(8).Italic().FontColor(Colors.Grey.Darken2);
                        }

                        // Supplier summary table (tot_print's "SUPPLIER CODE / VALUE" box).
                        if (report.SupplierTotals.Count > 0)
                        {
                            column.Item().PaddingTop(12).Text("Supplier Value Summary").Bold().FontSize(10);
                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(100);
                                });
                                table.Header(header =>
                                {
                                    header.Cell().Text("Supplier Code").Bold().FontSize(8);
                                    header.Cell().Text("Supplier Name").Bold().FontSize(8);
                                    header.Cell().Text($"Value ({report.CurrencyCode})").Bold().FontSize(8);
                                    header.Cell().ColumnSpan(3).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                                });
                                foreach (var supplier in report.SupplierTotals)
                                {
                                    table.Cell().PaddingVertical(2).Text(supplier.SupplierCode).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(supplier.SupplierName).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(supplier.TotalValue.ToString("#,##0.00")).FontSize(8);
                                }
                            });
                        }

                        // Grand total.
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text($"Total Value - {report.CurrencyCode}").Bold().FontSize(10);
                            row.ConstantItem(120).AlignRight().Text(report.GrandTotalValue.ToString("#,##0.000")).Bold().FontSize(10);
                        });

                        // Estimated Profit - only present when the caller was authorized
                        // (mirrors legacy's access('trimprof')).
                        if (report.Profit != null)
                        {
                            var profit = report.Profit;
                            column.Item().PaddingTop(10).Background(Colors.Yellow.Lighten4).Padding(6).Column(profitColumn =>
                            {
                                profitColumn.Item().Text("Estimated Profit (restricted view)").Bold().FontSize(9);
                                profitColumn.Item().PaddingTop(2).Text($"Unit Price per Garment: {profit.UnitPricePerGarment:#,##0.000}").FontSize(8);
                                profitColumn.Item().Text($"Cost per Garment: {profit.CostPerGarment:#,##0.000} ({profit.CostPercentageOfUnitPrice:0.000}% of Unit Price)").FontSize(8);
                                profitColumn.Item().Text($"Estimated Profit per Garment: {profit.EstimatedProfitPerGarment:#,##0.000} ({profit.EstimatedProfitPercentage:0.000}%)")
                                    .FontSize(8).Bold()
                                    .FontColor(profit.EstimatedProfitPerGarment < 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                            });
                        }

                        // Approval stamp.
                        column.Item().PaddingTop(10);
                        if (report.ApprovalStamp != null)
                        {
                            column.Item().Text($"Approved by {report.ApprovalStamp.ApprovedByUserId} on {report.ApprovalStamp.ApprovedDate:dd-MMM-yyyy}")
                                .FontSize(9).Bold().FontColor(Colors.Green.Darken4);
                        }
                        else
                        {
                            column.Item().Text("Not yet approved.").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                        }

                        // ═══════════════════════════════════════════════════════════════
                        // 🎓 PHASE 2 STEP 5 — "Terms & Conditions" section.
                        //
                        // Renders context-matched SOPs as a formal T&C appendix at the
                        // bottom of the Trim Sheet PDF. Only appears when at least one
                        // SOP matched the report's buyer/supplier/report-type context
                        // via GetActiveSopsForContextAsync.
                        //
                        // LAYOUT per SOP:
                        //   ┌─────────────────────────────────────────────────┐
                        //   │ [SOP-TRIM-001] Title of the Procedure          │ Bold heading
                        //   │ Category: Quality                              │ Muted label
                        //   │ One-line description / summary                 │ Italic
                        //   │                                                │
                        //   │ Full procedure text, may span multiple lines.  │ Body
                        //   │ Paragraph breaks preserved via Split('\n').     │
                        //   └─────────────────────────────────────────────────┘
                        //
                        // The section title uses the same Blue.Darken4 accent as the
                        // report header to maintain visual consistency.
                        // ═══════════════════════════════════════════════════════════════
                        if (report.ApplicableSops.Count > 0)
                        {
                            // 🎓 Section divider — a thick blue line matching the header
                            // to visually separate the financial content above from the
                            // procedural/compliance content below.
                            column.Item().PaddingTop(16).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);

                            column.Item().PaddingTop(8).Text("Terms & Conditions")
                                .FontSize(12).Bold().FontColor(Colors.Blue.Darken4);

                            column.Item().PaddingTop(2).Text(
                                "The following Standard Operating Procedures apply to this order based on the buyer, supplier, and report context.")
                                .FontSize(8).Italic().FontColor(Colors.Grey.Darken2);

                            foreach (var sop in report.ApplicableSops)
                            {
                                // 🎓 Each SOP rendered as a bordered block with light
                                // background to distinguish individual procedures visually.
                                column.Item().PaddingTop(8).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                    .Background(Colors.Grey.Lighten4).Padding(8).Column(sopColumn =>
                                {
                                    // 🎓 SOP header: code + title on the same line.
                                    // Code in brackets serves as a reference identifier
                                    // so the reader can look up the full procedure in the
                                    // SOP management screen if needed.
                                    sopColumn.Item().Text($"[{sop.SopCode}] {sop.Title}")
                                        .FontSize(9).Bold();

                                    // 🎓 Category label — muted, small, right below the title.
                                    if (!string.IsNullOrWhiteSpace(sop.Category))
                                    {
                                        sopColumn.Item().PaddingTop(2).Text($"Category: {sop.Category}")
                                            .FontSize(7).FontColor(Colors.Grey.Darken1);
                                    }

                                    // 🎓 Description — one-line summary in italic.
                                    if (!string.IsNullOrWhiteSpace(sop.Description))
                                    {
                                        sopColumn.Item().PaddingTop(3).Text(sop.Description)
                                            .FontSize(8).Italic();
                                    }

                                    // 🎓 Full text body — the complete procedure content.
                                    // Split by newlines to preserve paragraph structure
                                    // from the SOP's FullText field (admin may have entered
                                    // multi-paragraph instructions).
                                    if (!string.IsNullOrWhiteSpace(sop.FullText))
                                    {
                                        sopColumn.Item().PaddingTop(4).LineHorizontal(0.5f)
                                            .LineColor(Colors.Grey.Lighten1);

                                        var paragraphs = sop.FullText.Split('\n',
                                            StringSplitOptions.RemoveEmptyEntries);

                                        foreach (var paragraph in paragraphs)
                                        {
                                            sopColumn.Item().PaddingTop(3).Text(paragraph.Trim())
                                                .FontSize(8);
                                        }
                                    }
                                });
                            }
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("[ End of Report ]").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            // NOTE: FontSize must be set inside the delegate via DefaultTextStyle -
                            // Text(Action<TextDescriptor>) returns void, so it cannot be chained
                            // after the call (mirrors the established pattern in StrnPrintEngine.cs).
                            x.DefaultTextStyle(TextStyle.Default.FontSize(8));
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
