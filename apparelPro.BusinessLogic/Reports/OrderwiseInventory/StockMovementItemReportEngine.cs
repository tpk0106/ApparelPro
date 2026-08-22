using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Physically lives alongside StockMovementReportEngine - same project location
    // convention, same namespace matching the engine's actual physical/project location.
    public static class StockMovementItemReportEngine
    {
        // Same movement-direction color convention as StockMovementReportEngine - never
        // reused as a generic accent.
        private const string InboundColor = "#0CA30C";
        private const string OutboundColor = "#D03B3B";
        private const string NeutralColor = "#8B93A1";

        public static byte[] GenerateStockMovementItemReportPdf(
            StockMovementItemReportHeaderServiceModel header,
            List<StockMovementItemReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Portrait, unlike the sibling Order-level report's landscape layout -
                    // this report has far fewer columns (a ledger, not a wide totals grid).
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ STOCK MOVEMENT REPORT - FOR AN ITEM ]").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Buyer : {header.BuyerCode} - {header.BuyerName}        Order : {header.Order}");
                        column.Item().PaddingTop(2).Text($"Item : {header.ItemCode} - {header.Description}        Unit : {header.Unit}        Order Qty : {header.OrderQuantity:N2}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Date
                            columns.RelativeColumn(2);  // Doc. No
                            columns.RelativeColumn(4);  // Document Name
                            columns.RelativeColumn(2);  // Amount
                            columns.RelativeColumn(2);  // Balance
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[] { "Date", "Doc. No", "Document Name", "Amount", "Balance" })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(8);
                            }
                        });

                        foreach (var line in lines)
                        {
                            var color = line.TransactionType switch
                            {
                                "GR" or "0X" or "1T" or "2R" => InboundColor,
                                "4I" or "5D" or "6T" or "7S" => OutboundColor,
                                _ => NeutralColor,
                            };

                            table.Cell().Padding(3).Text($"{line.TransactionDate:dd/MM/yyyy}");
                            table.Cell().Padding(3).Text(line.DocumentNumber);
                            table.Cell().Padding(3).Text(line.TransactionTypeName);
                            table.Cell().Padding(3).AlignRight().Text($"{line.Quantity:N2}").FontColor(color);
                            table.Cell().Padding(3).AlignRight().Text($"{line.BalanceAfter:N2}").Bold();
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().AlignCenter().PaddingTop(4).Text($"[ End of Report ]   Closing Balance : {header.ClosingBalance:N2}").Bold();
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
