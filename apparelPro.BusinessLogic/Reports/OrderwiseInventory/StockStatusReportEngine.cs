using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SSTAT.PRG's printed report.
    public static class StockStatusReportEngine
    {
        public static byte[] GenerateStockStatusReportPdf(
            StockStatusReportHeaderServiceModel header,
            List<StockStatusReportLineServiceModel> lines)
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
                        column.Item().AlignCenter().Text("Stock Status Report").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Buyer : {header.BuyerName}    Order : {header.Order}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(3);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(1.5f); // Order Qty
                            columns.RelativeColumn(1.5f); // Received Qty
                            columns.RelativeColumn(1.5f); // Bal to Receive
                            columns.RelativeColumn(1.5f); // Damaged Qty
                            columns.RelativeColumn(1.5f); // Qty in Hand
                            columns.RelativeColumn(1);  // Basis
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "Order Qty", "Received Qty",
                                "Bal. to Receive", "Damaged Qty", "Qty in Hand", "Basis"
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
                            table.Cell().Padding(3).AlignRight().Text($"{line.OrderedQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.BalanceToReceive:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.DamagedQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.QtyInHand:N2}");
                            table.Cell().Padding(3).Text(line.StoreCode);
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
