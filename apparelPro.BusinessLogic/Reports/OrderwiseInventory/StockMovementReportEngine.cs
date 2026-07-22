using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Physically lives in apparelPro.BusinessLogic, alongside StylewiseEventReportEngine.
    // Note: StylewiseEventReportEngine.cs is namespaced "ApparelPro.WebApi.Reports.OrderManagement.Stylewise_Events"
    // despite living in this project — that mismatch isn't repeated here; this
    // namespace matches the engine's actual physical/project location.
    public static class StockMovementReportEngine
    {
        // Status colors reserved for movement direction — never reused as a generic accent.
        private const string InboundColor = "#0CA30C";
        private const string OutboundColor = "#D03B3B";
        private const string DiscrepancyFill = "#FAB219";
        private const string DiscrepancyText = "#111111";

        public static byte[] GenerateStockMovementReportPdf(
            StockMovementReportHeaderServiceModel header,
            List<StockMovementReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    // Replaces the legacy manual line-counter/'Contd.../eject pagination
                    // (ln >= 60, m_pgcnt) — QuestPDF repeats this header on every page automatically.
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ STOCK MOVEMENT REPORT ]").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Buyer : {header.BuyerName}        Order : {header.Order}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(3);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(2);  // Order Qty
                            columns.RelativeColumn(2);  // Received
                            columns.RelativeColumn(2);  // Requested
                            columns.RelativeColumn(2);  // Issued
                            columns.RelativeColumn(2);  // Trans In
                            columns.RelativeColumn(2);  // Trans Out
                            columns.RelativeColumn(2);  // Damaged
                            columns.RelativeColumn(2);  // Ret. Supp
                            columns.RelativeColumn(2);  // Last Adj
                            columns.RelativeColumn(2);  // Qty in Hand
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "Order Qty", "Received",
                                "Requested", "Issued", "Trans. In", "Trans. Out", "Damaged",
                                "Ret.Supp.", "Last Adj.", "Qty in Hand"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(8);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.ItemCode);
                            table.Cell().Padding(3).Text(line.Description);
                            table.Cell().Padding(3).Text(line.Unit);
                            table.Cell().Padding(3).AlignRight().Text($"{line.OrderedQuantity:N2}");
                            QuantityCell(table, line.ReceivedQuantity, InboundColor);
                            table.Cell().Padding(3).AlignRight().Text($"{line.RequisitionedQuantity:N2}");
                            QuantityCell(table, line.IssuedQuantity, OutboundColor);
                            QuantityCell(table, line.TransferInQuantity, InboundColor);
                            QuantityCell(table, line.TransferOutQuantity, OutboundColor);
                            QuantityCell(table, line.DamagedQuantity, null, DiscrepancyFill);
                            QuantityCell(table, line.SupplierReturnQuantity, OutboundColor);
                            QuantityCell(table, line.LastAdjustmentQuantity, null, DiscrepancyFill);
                            table.Cell().Padding(3).AlignRight().Text($"{line.BalanceQuantity:N2}").Bold();
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().AlignCenter().PaddingTop(4).Text("[ End of Report ]").Bold();
                        column.Item().AlignRight().Text(x =>
                        {
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

        // Discrepancy cells (Damaged / Last Adjustment) use a filled amber badge rather
        // than amber text — amber-on-white fails WCAG contrast for body text, so dark
        // text on an amber fill keeps the discrepancy signal readable in print, while
        // still visually distinct from the plain inbound/outbound colored text cells.
        private static void QuantityCell(TableDescriptor table, decimal value, string? textColor, string? fillColor = null)
        {
            var cell = table.Cell().Padding(3).AlignRight();
            if (fillColor is not null)
                cell.Background(fillColor).Text($"{value:N2}").FontColor(DiscrepancyText).Bold();
            else if (textColor is not null && value != 0)
                cell.Text($"{value:N2}").FontColor(textColor).Bold();
            else
                cell.Text($"{value:N2}");
        }
    }
}
