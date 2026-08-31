using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_DLIST.PRG's printed report.
    public static class GeneralTransactionListReportEngine
    {
        public static byte[] GenerateTransactionListReportPdf(
            GeneralTransactionListReportHeaderServiceModel header,
            List<GeneralTransactionListReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            string subtitle = $"Transactions from {header.FromDate:dd/MM/yyyy} to {header.ToDate:dd/MM/yyyy}";

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
                        column.Item().AlignCenter().Text("List of Transactions (General)").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text(subtitle);
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);  // Type
                            columns.RelativeColumn(2);  // Date
                            columns.RelativeColumn(2);  // Time
                            columns.RelativeColumn(2);  // Doc No
                            columns.RelativeColumn(1);  // Store
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(4);  // Description
                            columns.RelativeColumn(2);  // Quantity
                            columns.RelativeColumn(1);  // Unit
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var col in new[]
                            {
                                "Type", "Date", "Time", "Doc No", "Store", "Item Code", "Description", "Quantity", "Unit"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(col).Bold().FontSize(7.5f);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.DocumentTypeDescription);
                            table.Cell().Padding(3).Text(line.TransactionDate.ToString("dd/MM/yy"));
                            table.Cell().Padding(3).Text(line.TransactionTime?.ToString("HH:mm:ss") ?? "");
                            table.Cell().Padding(3).Text(line.DocumentNumber);
                            table.Cell().Padding(3).Text(line.StoreCode);
                            table.Cell().Padding(3).Text(line.ItemCode);
                            table.Cell().Padding(3).Text(line.Description);
                            table.Cell().Padding(3).AlignRight().Text($"{line.Quantity:N2}");
                            table.Cell().Padding(3).Text(line.Unit);
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
