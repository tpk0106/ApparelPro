using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_RMCON.PRG's printed report.
    public static class RawMaterialControlSheetEngine
    {
        public static byte[] GenerateRawMaterialControlSheetPdf(
            RawMaterialControlSheetHeaderServiceModel header,
            List<RawMaterialControlSheetLineServiceModel> lines)
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
                        column.Item().AlignCenter().Text("Raw Material Control Sheet").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text($"Buyer : {header.BuyerCode}    Order : {header.Order}    Item : {header.ItemDescription}    Qty : {header.OrderQuantity:N2} {header.Unit}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.2f); // Item Code
                            columns.RelativeColumn(3);   // Description
                            columns.RelativeColumn(0.8f); // Unit
                            columns.RelativeColumn(1.3f); // Total Consumption
                            columns.RelativeColumn(1.3f); // Exact Consumption
                            columns.RelativeColumn(1.3f); // Order Qty
                            columns.RelativeColumn(1.3f); // Received Qty
                            columns.RelativeColumn(1.3f); // Issued Qty
                            columns.RelativeColumn(1.3f); // Qty In Hand
                            columns.RelativeColumn(1.1f); // Unit Price
                            columns.RelativeColumn(0.8f); // Curr
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "Total Consumption", "Exact Consumption",
                                "Order Qty", "Received Qty", "Issued Qty", "Qty In Hand", "Unit Price", "Curr"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).Text(title).Bold().FontSize(7);
                            }
                        });

                        string? lastStockCode = null;
                        foreach (var line in lines)
                        {
                            if (line.StockCode != lastStockCode)
                            {
                                table.Cell().ColumnSpan(11).PaddingTop(3).Text(line.StockDescription).Bold().Underline();
                                lastStockCode = line.StockCode;
                            }

                            table.Cell().Padding(2).Text(line.ItemCode);
                            table.Cell().Padding(2).Text(line.Description);
                            table.Cell().Padding(2).Text(line.Unit);
                            table.Cell().Padding(2).AlignRight().Text($"{line.TotalConsumption:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.ExactConsumption:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.TotalOrderQuantity:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.TotalReceivedQuantity:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.TotalIssuedQuantity:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.QtyInHand:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.UnitPrice:N2}");
                            table.Cell().Padding(2).Text(line.Currency);
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
