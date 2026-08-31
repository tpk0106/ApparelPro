using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SVAL1.PRG's printed report.
    public static class StockValuationMonthlyReportEngine
    {
        public static byte[] GenerateStockValuationMonthlyReportPdf(
            StockValuationMonthlyReportHeaderServiceModel header,
            List<StockValuationMonthlyReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("Stock Valuation Report (Monthly)").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"From {header.FromDate:dd/MM/yyyy} to {header.ToDate:dd/MM/yyyy}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(3);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(1);  // Curr
                            columns.RelativeColumn(1.5f); // U/Price
                            columns.RelativeColumn(1.5f); // Received Qty
                            columns.RelativeColumn(1.8f); // Received Value
                            columns.RelativeColumn(1.5f); // Issued Qty
                            columns.RelativeColumn(1.8f); // Issued Value
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "Curr", "U/Price", "Received Qty",
                                "Received Value", "Issued Qty", "Issued Value"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(7.5f);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.ItemCode);
                            table.Cell().Padding(3).Text(line.Description);
                            table.Cell().Padding(3).Text(line.Unit);
                            table.Cell().Padding(3).Text(line.Currency);
                            table.Cell().Padding(3).AlignRight().Text($"{line.UnitPrice:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedValue:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.IssuedQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.IssuedValue:N2}");
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().AlignRight().PaddingTop(4).Text(t =>
                        {
                            t.Span("Total Received Value : ").Bold();
                            t.Span($"{header.TotalReceivedValue:N2}").Bold();
                            t.Span("     Total Issued Value : ").Bold();
                            t.Span($"{header.TotalIssuedValue:N2}").Bold();
                        });
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
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
