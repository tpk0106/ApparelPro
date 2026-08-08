using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderDetailReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.OrderDetail
{
    // Replicates OD_RPO1.PRG's "ORDER CONFIRMATION REPORT" print program, scoped to the
    // prn_for1 branch (Buyer+Order given, Type blank) per explicit project decision
    // (2026-08-07): one full detail printout per Buyer+Order, listing every Style under it
    // expanded into its Part-Shipment lines. See OrderDetailReportServiceModel's SCOPE NOTE
    // for the two legacy fields with no modern equivalent (order Description; resolved
    // Destination description).
    public static class OrderDetailReportEngine
    {
        public static byte[] GeneratePdf(OrderDetailReportServiceModel report)
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
                            row.RelativeItem().Text("[ O R D E R   C O N F I R M A T I O N ]").FontSize(14).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Buyer: ").Bold(); t.Span(string.IsNullOrEmpty(report.BuyerName) ? report.BuyerCode.ToString() : report.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order: ").Bold(); t.Span(report.Order); });
                            row.RelativeItem().Text(t => { t.Span("Order Date: ").Bold(); t.Span(report.OrderDate.ToString("dd-MMM-yyyy")); });
                        });
                        column.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Unit: ").Bold(); t.Span(report.Unit); });
                            row.RelativeItem().Text(t => { t.Span("Currency: ").Bold(); t.Span(report.CurrencyCode); });
                            row.RelativeItem();
                        });

                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        // One block per Style, in order, each expanded into its Part-Shipment
                        // lines - mirrors OD_RPO1.PRG's prn_for1 layout (style header line
                        // followed by its shipment breakdown, then a style subtotal).
                        foreach (var style in report.Styles)
                        {
                            column.Item().PaddingTop(10).Text($"Style: {style.StyleCode}").Bold().Underline().FontSize(10);
                            column.Item().PaddingTop(2).Row(row =>
                            {
                                row.RelativeItem().Text(t => { t.Span("Type: ").Bold(); t.Span(string.IsNullOrEmpty(style.TypeName) ? style.TypeCode.ToString() : style.TypeName); });
                                row.RelativeItem().Text(t => { t.Span("Qty: ").Bold(); t.Span($"{style.Quantity:#,##0.##} {style.Unit}"); });
                                row.RelativeItem().Text(t => { t.Span("Unit Price: ").Bold(); t.Span(style.UnitPrice.ToString("#,##0.00")); });
                            });

                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.5f); // New Order
                                    columns.RelativeColumn(2f);   // Destination
                                    columns.ConstantColumn(70);   // Ship Date
                                    columns.ConstantColumn(80);   // Qty
                                    columns.ConstantColumn(80);   // Value
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("New Order").Bold().FontSize(8);
                                    header.Cell().Text("Destination").Bold().FontSize(8);
                                    header.Cell().Text("Ship Date").Bold().FontSize(8);
                                    header.Cell().Text($"Qty ({style.Unit})").Bold().FontSize(8);
                                    header.Cell().Text($"Value ({report.CurrencyCode})").Bold().FontSize(8);
                                    header.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                                });

                                foreach (var part in style.PartShipments)
                                {
                                    table.Cell().PaddingVertical(2).Text(part.NewOrder).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(part.DestinationCode).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(part.ShipDate.ToString("dd-MMM-yyyy")).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(part.Quantity.ToString("#,##0.##")).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(part.Value.ToString("#,##0.00")).FontSize(8);
                                }
                            });

                            // Style subtotal - only shown when there's more than one shipment
                            // line, mirroring TrimSheetReportEngine's per-group subtotal pattern.
                            if (style.PartShipments.Count > 1)
                            {
                                column.Item().PaddingTop(3).Row(row =>
                                {
                                    row.RelativeItem().Text($"Total for Style {style.StyleCode}").FontSize(8).Italic();
                                    row.ConstantItem(100).AlignRight().Text($"Qty: {style.TotalQuantity:#,##0.##}  Value: {style.TotalValue:#,##0.00}").Bold().FontSize(8);
                                });
                            }
                        }

                        // Order grand total.
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text($"Grand Total Value - {report.CurrencyCode}").Bold().FontSize(10);
                            row.ConstantItem(120).AlignRight().Text(report.GrandTotalValue.ToString("#,##0.00")).Bold().FontSize(10);
                        });
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("[ End of Report ]").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            // NOTE: FontSize must be set inside the delegate via DefaultTextStyle -
                            // Text(Action<TextDescriptor>) returns void, so it cannot be chained
                            // after the call (mirrors the established pattern in TrimSheetReportEngine.cs).
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
