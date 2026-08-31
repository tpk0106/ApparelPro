using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_ROL.PRG's printed report.
    public static class GeneralStockReorderReportEngine
    {
        public static byte[] GenerateStockReorderReportPdf(
            GeneralStockReorderReportHeaderServiceModel header,
            List<GeneralStockReorderReportLineServiceModel> lines)
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
                        column.Item().AlignCenter().Text("Stock Re-order Report").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Stores : {header.StoreCode} - {header.StoreDescription}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(3);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(2);  // Ave. Price
                            columns.RelativeColumn(2);  // Qty In Hand
                            columns.RelativeColumn(2);  // Re-order Level
                            columns.RelativeColumn(2);  // Re-order Qty
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "Ave. Price", "Qty In Hand", "Re-order Level", "Re-order Qty"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(8);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.ItemCode);
                            table.Cell().Padding(3).Text(line.Description);
                            table.Cell().Padding(3).Text(line.Unit);
                            table.Cell().Padding(3).AlignRight().Text($"{line.AveragePrice:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.QtyInHand:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReorderLevel:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.ReorderQuantity:N2}");
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
