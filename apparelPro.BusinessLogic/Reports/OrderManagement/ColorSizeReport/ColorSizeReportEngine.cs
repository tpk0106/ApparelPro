using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.ColorSizeReport
{
    // Replicates OD_CLSZ3.PRG's "COLOUR / SIZE DETAILS" printed layout: one pivot table
    // per Style (rows = Colour, columns = every distinct Size across the whole
    // Buyer+Order), with a Total row per Style. Landscape orientation mirrors legacy's
    // explicit 132-column-wide print job (`do scr_prn with '...', [132]`) - this report
    // can have many Size columns, unlike Trim Sheet/Order Detail's portrait layout.
    public static class ColorSizeReportEngine
    {
        public static byte[] GeneratePdf(ColorSizeReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ C O L O U R  /  S I Z E   D E T A I L S ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Row(row =>
                        {
                            // FIXED (2026-08-08): report screens show the resolved name only,
                            // never "code - name" together - falls back to the raw code only
                            // when the name itself is blank/unresolved.
                            row.RelativeItem().Text(t => { t.Span("Buyer: ").Bold(); t.Span(string.IsNullOrEmpty(report.BuyerName) ? report.BuyerCode.ToString() : report.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order: ").Bold(); t.Span(report.Order); });
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        foreach (var style in report.Styles)
                        {
                            column.Item().PaddingTop(10).Text($"Style: {style.StyleCode}").Bold().Underline().FontSize(10);

                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(60);   // Colour
                                    columns.RelativeColumn(2f);   // Description
                                    foreach (var _ in report.SizeColumns)
                                        columns.ConstantColumn(45); // one per Size
                                    columns.ConstantColumn(55);   // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Colour").Bold().FontSize(7);
                                    header.Cell().Text("Description").Bold().FontSize(7);
                                    foreach (var size in report.SizeColumns)
                                        header.Cell().AlignRight().Text(size).Bold().FontSize(7);
                                    header.Cell().AlignRight().Text("Total").Bold().FontSize(7);
                                    header.Cell().ColumnSpan((uint)(3 + report.SizeColumns.Count)).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                                });

                                foreach (var colour in style.Colours)
                                {
                                    table.Cell().PaddingVertical(2).Text(colour.ColorCode).FontSize(7);
                                    table.Cell().PaddingVertical(2).Text(colour.Description).FontSize(7);
                                    foreach (var size in report.SizeColumns)
                                    {
                                        var qty = colour.SizeQuantities.TryGetValue(size, out var v) ? v : 0;
                                        table.Cell().PaddingVertical(2).AlignRight().Text(qty == 0 ? "-" : qty.ToString("#,##0.##")).FontSize(7);
                                    }
                                    table.Cell().PaddingVertical(2).AlignRight().Text(colour.TotalQuantity.ToString("#,##0.##")).Bold().FontSize(7);
                                }

                                // Style Total row.
                                table.Cell().PaddingTop(3).Text("Total").Bold().FontSize(7);
                                table.Cell().PaddingTop(3).Text("");
                                foreach (var size in report.SizeColumns)
                                {
                                    var total = style.SizeTotals.TryGetValue(size, out var v) ? v : 0;
                                    table.Cell().PaddingTop(3).AlignRight().Text(total.ToString("#,##0.##")).Bold().FontSize(7);
                                }
                                table.Cell().PaddingTop(3).AlignRight().Text(style.GrandTotal.ToString("#,##0.##")).Bold().FontSize(7);
                            });
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("[ End of Report ]").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
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
