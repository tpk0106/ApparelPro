using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SVAL2.PRG's printed report.
    public static class StockSummaryReportEngine
    {
        public static byte[] GenerateStockSummaryReportPdf(
            StockSummaryReportHeaderServiceModel header,
            List<StockSummaryReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("Summary of Stock Value - Basis Wise").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Currencies : {header.Currency1} / {header.Currency2}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);  // Stock Type / Store
                            columns.RelativeColumn(3);  // Value in Currency1
                            columns.RelativeColumn(3);  // Value in Currency2
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Stock Type / Store").Bold().FontSize(8);
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text($"Value in {header.Currency1}").Bold().FontSize(8);
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text($"Value in {header.Currency2}").Bold().FontSize(8);
                        });

                        foreach (var line in lines)
                        {
                            switch (line.RowType)
                            {
                                case "Store":
                                    table.Cell().Padding(3).PaddingLeft(15).Text(line.StoreCode);
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ValueInCurrency1:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ValueInCurrency2:N2}");
                                    break;
                                case "StockTypeSubtotal":
                                    table.Cell().Padding(3).Text($"{line.StockTypeCode} - {line.StockTypeDescription} (Sub Total)").Bold();
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ValueInCurrency1:N2}").Bold();
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ValueInCurrency2:N2}").Bold();
                                    break;
                                case "GrandTotal":
                                    table.Cell().Padding(3).Text("GRAND TOTAL").Bold().FontSize(11);
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ValueInCurrency1:N2}").Bold().FontSize(11);
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ValueInCurrency2:N2}").Bold().FontSize(11);
                                    break;
                            }
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
    }
}
