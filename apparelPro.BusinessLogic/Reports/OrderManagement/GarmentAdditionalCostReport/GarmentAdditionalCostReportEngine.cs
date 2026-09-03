using apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.GarmentAdditionalCostReport
{
    // Replicates OD_AITM2.PRG's "Additional Costs per Garment" print layout - one table
    // per Additional Cost category (od_acost.dbf), each row a garment-cost detail line
    // (od_aitm.dbf), with a per-category subtotal and an overall grand total. Matches the
    // established landscape flat-table pattern used by TrimSheetReportEngine.
    public static class GarmentAdditionalCostReportEngine
    {
        public static byte[] GeneratePdf(GarmentAdditionalCostReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ ADDITIONAL COSTS PER GARMENT ]").FontSize(14).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Buyer: ").Bold(); t.Span(string.IsNullOrEmpty(report.BuyerName) ? report.BuyerCode.ToString() : report.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order: ").Bold(); t.Span(report.Order); });
                            row.RelativeItem().Text(t => { t.Span("Type: ").Bold(); t.Span(string.IsNullOrEmpty(report.TypeName) ? report.TypeCode.ToString() : report.TypeName); });
                            row.RelativeItem().Text(t => { t.Span("Style: ").Bold(); t.Span(report.StyleCode); });
                        });

                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        decimal grandTotal = 0;

                        foreach (var category in report.Categories)
                        {
                            grandTotal += category.TotalValue;

                            column.Item().PaddingTop(8).Text($"{category.AdditionalCostCode} - {category.AdditionalCostName}").Bold().Underline().FontSize(10);

                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.5f); // Item code / description
                                    columns.ConstantColumn(50);   // Colour
                                    columns.ConstantColumn(50);   // Size
                                    columns.ConstantColumn(60);   // Quantity
                                    columns.ConstantColumn(90);   // Store/Basis
                                    columns.ConstantColumn(60);   // Currency
                                    columns.ConstantColumn(70);   // Price
                                    columns.ConstantColumn(80);   // Value
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Item Code / Description").Bold().FontSize(8);
                                    header.Cell().Text("Colour").Bold().FontSize(8);
                                    header.Cell().Text("Size").Bold().FontSize(8);
                                    header.Cell().Text("Quantity").Bold().FontSize(8);
                                    header.Cell().Text("Basis").Bold().FontSize(8);
                                    header.Cell().Text("Curr.").Bold().FontSize(8);
                                    header.Cell().Text("Price").Bold().FontSize(8);
                                    header.Cell().Text("Value").Bold().FontSize(8);
                                    header.Cell().ColumnSpan(8).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                                });

                                foreach (var line in category.Lines)
                                {
                                    table.Cell().PaddingVertical(2).Text($"{line.ItemCode} - {line.Description}").FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.Color).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.Size).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text($"{line.Quantity:#,##0.###} {line.Unit}").FontSize(8);
                                    table.Cell().PaddingVertical(2).Text($"{line.StoreCode} - {line.StoreName}").FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.Currency).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.Price.ToString("#,##0.0000")).FontSize(8);
                                    table.Cell().PaddingVertical(2).Text(line.Value.ToString("#,##0.00")).FontSize(8);
                                }
                            });

                            column.Item().PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem();
                                row.ConstantItem(150).AlignRight().Text($"Category Total: {category.TotalValue:#,##0.00}").Bold().FontSize(8);
                            });
                        }

                        if (report.Categories.Count == 0)
                        {
                            column.Item().PaddingTop(10).Text("No Additional Costs recorded for this style.").Italic().FontSize(9).FontColor(Colors.Grey.Darken2);
                        }

                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text("Grand Total").Bold().FontSize(10);
                            row.ConstantItem(150).AlignRight().Text(grandTotal.ToString("#,##0.00")).Bold().FontSize(10);
                        });
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
