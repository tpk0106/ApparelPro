using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SVAL.PRG's printed report.
    public static class StockValuationReportEngine
    {
        public static byte[] GenerateStockValuationReportPdf(
            StockValuationReportHeaderServiceModel header,
            List<StockValuationReportLineServiceModel> lines)
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
                        column.Item().AlignCenter().Text("Stock Valuation Report").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Buyer : {header.BuyerName}    Order : {header.Order}    Currency : {header.Currency}");
                        foreach (var style in header.Styles)
                        {
                            column.Item().Text($"Style : {style.StyleCode}    Quantity : {style.Quantity:N2} {style.Unit}    Price : {style.UnitPrice:N2}").FontSize(7.5f);
                        }
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(3);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(1.5f); // U/Price
                            columns.RelativeColumn(1.5f); // Order Qty
                            columns.RelativeColumn(1.5f); // Received Qty
                            columns.RelativeColumn(1.8f); // Received Value
                            columns.RelativeColumn(1.5f); // Issued Qty
                            columns.RelativeColumn(1.8f); // Issued Value
                            columns.RelativeColumn(1.5f); // Qty In Hand
                            columns.RelativeColumn(1.8f); // Balance Value
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", $"U/Price({header.Currency})", "Order Qty",
                                "Received Qty", "Received Value", "Issued Qty", "Issued Value", "Qty In Hand", "Balance Value"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(7);
                            }
                        });

                        foreach (var line in lines)
                        {
                            switch (line.RowType)
                            {
                                case "Item":
                                    table.Cell().Padding(3).Text(line.ItemCode);
                                    table.Cell().Padding(3).Text(line.Description);
                                    table.Cell().Padding(3).Text(line.Unit);
                                    table.Cell().Padding(3).AlignRight().Text($"{line.UnitPrice:N4}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.OrderedQuantity:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedQuantity:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedValue:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.IssuedQuantity:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.IssuedValue:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.QtyInHand:N2}");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.BalanceValue:N2}");
                                    break;
                                case "StockTypeSubtotal":
                                    table.Cell().ColumnSpan(6).Padding(3).Text($"Total Value For {line.StockTypeCode} - {line.StockTypeDescription}").Bold();
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedValue:N2}").Bold();
                                    table.Cell().Padding(3).Text("");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.IssuedValue:N2}").Bold();
                                    table.Cell().Padding(3).Text("");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.BalanceValue:N2}").Bold();
                                    break;
                                case "GrandTotal":
                                    table.Cell().ColumnSpan(6).Padding(3).Text("TOTAL VALUES").Bold().FontSize(10);
                                    table.Cell().Padding(3).AlignRight().Text($"{line.ReceivedValue:N2}").Bold().FontSize(10);
                                    table.Cell().Padding(3).Text("");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.IssuedValue:N2}").Bold().FontSize(10);
                                    table.Cell().Padding(3).Text("");
                                    table.Cell().Padding(3).AlignRight().Text($"{line.BalanceValue:N2}").Bold().FontSize(10);
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
