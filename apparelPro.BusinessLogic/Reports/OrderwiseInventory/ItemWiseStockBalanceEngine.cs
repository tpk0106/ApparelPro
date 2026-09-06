using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_STBAL.PRG's printed report.
    public static class ItemWiseStockBalanceEngine
    {
        public static byte[] GenerateItemWiseStockBalancePdf(
            ItemWiseStockBalanceHeaderServiceModel header,
            List<ItemWiseStockBalanceLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.3f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7.5f));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("Item-wise Stock Balances (Order-wise)").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text($"Range : {header.FromRange} to {header.ToRange}    Currency : {header.Currency}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);   // Buyer
                            columns.RelativeColumn(1.5f); // Order
                            columns.RelativeColumn(2.2f); // Item Code
                            columns.RelativeColumn(3);   // Description
                            columns.RelativeColumn(0.8f); // Unit
                            columns.RelativeColumn(1.3f); // Unit Price
                            columns.RelativeColumn(1.3f); // Received Qty
                            columns.RelativeColumn(1.5f); // Received Value
                            columns.RelativeColumn(1.3f); // Qty In Hand
                            columns.RelativeColumn(1.5f); // Balance Value
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Buyer", "Order", "Item Code", "Description", "Unit", "Unit Price",
                                "Received Qty", "Received Value", "Qty In Hand", "Balance Value"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).Text(title).Bold().FontSize(7);
                            }
                        });

                        foreach (var line in lines)
                        {
                            switch (line.RowType)
                            {
                                case "Item":
                                    table.Cell().Padding(2).Text(line.BuyerName);
                                    table.Cell().Padding(2).Text(line.Order);
                                    table.Cell().Padding(2).Text(line.ItemCode);
                                    table.Cell().Padding(2).Text(line.Description);
                                    table.Cell().Padding(2).Text(line.Unit);
                                    table.Cell().Padding(2).AlignRight().Text($"{line.UnitPrice:N4}");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.ReceivedQuantity:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.ReceivedValue:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.QtyInHand:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.BalanceValue:N2}");
                                    break;
                                case "ItemGroupSubtotal":
                                    table.Cell().ColumnSpan(7).Padding(2).Text($"Total for {line.ItemGroupCode} - {line.ItemGroupDescription}").Bold();
                                    table.Cell().Padding(2).AlignRight().Text($"{line.ReceivedValue:N2}").Bold();
                                    table.Cell().Padding(2).Text("");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.BalanceValue:N2}").Bold();
                                    break;
                                case "StockTypeSubtotal":
                                    table.Cell().ColumnSpan(7).Padding(2).Text($"Total for {line.StockTypeCode} - {line.StockTypeDescription}").Bold().FontSize(8.5f);
                                    table.Cell().Padding(2).AlignRight().Text($"{line.ReceivedValue:N2}").Bold().FontSize(8.5f);
                                    table.Cell().Padding(2).Text("");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.BalanceValue:N2}").Bold().FontSize(8.5f);
                                    break;
                                case "GrandTotal":
                                    table.Cell().ColumnSpan(7).Padding(2).Text("TOTAL VALUE").Bold().FontSize(10);
                                    table.Cell().Padding(2).AlignRight().Text($"{line.ReceivedValue:N2}").Bold().FontSize(10);
                                    table.Cell().Padding(2).Text("");
                                    table.Cell().Padding(2).AlignRight().Text($"{line.BalanceValue:N2}").Bold().FontSize(10);
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
