using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_SVAL.PRG's printed report.
    public static class GeneralStockValuationReportEngine
    {
        public static byte[] GenerateStockValuationReportPdf(
            GeneralStockValuationReportHeaderServiceModel header,
            List<GeneralStockValuationReportLineServiceModel> lines)
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
                        column.Item().AlignCenter().Text($"Stock Valuation Report upto {DateTime.Now:dd/MM/yyyy}").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Stores : {header.StoreCode} - {header.StoreDescription}    Item Range : {header.FromItemCode} to {header.ToItemCode}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);  // Item Code
                            columns.RelativeColumn(4);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(2);  // Qty in Hand
                            columns.RelativeColumn(2);  // Value
                            columns.RelativeColumn(2);  // Damaged Qty
                            columns.RelativeColumn(2);  // Re-order Level
                            columns.RelativeColumn(2);  // Re-order Qty
                            columns.RelativeColumn(2);  // Min Stock
                            columns.RelativeColumn(2);  // Max Stock
                            columns.RelativeColumn(2);  // Unit Price
                            columns.RelativeColumn(1);  // Currency
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "Qty in Hand", "Value", "Damaged Qty",
                                "Re-order Level", "Re-order Qty", "Min Stock", "Max Stock", "Unit Price", "Curr"
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
                            table.Cell().Padding(3).AlignRight().Text($"{line.QtyInHand:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.Value:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.DamagedQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReorderLevel:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReorderQuantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.MinStock:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.MaxStock:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.UnitPrice:N4}");
                            table.Cell().Padding(3).Text(line.Currency);
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().AlignRight().PaddingTop(4).Text($"Total Value : {header.TotalValue:N2}").Bold().FontSize(10);
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
