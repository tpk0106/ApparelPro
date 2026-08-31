using System.Globalization;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_SMOVE.PRG's printed report.
    public static class GeneralStockMovementReportEngine
    {
        public static byte[] GenerateStockMovementReportPdf(
            GeneralStockMovementReportHeaderServiceModel header,
            List<GeneralStockMovementReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            string monthLabel = new DateTime(header.Year, header.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text($"Stock Movement Report for {monthLabel}").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Stores : {header.StoreCode} - {header.StoreDescription}");
                        column.Item().Text($"Item : {header.ItemCode} - {header.ItemDescription}    Unit : {header.Unit}    B/F Balance from Last Month : {header.BroughtForwardBalance:N2}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Date
                            columns.RelativeColumn(2);  // Time
                            columns.RelativeColumn(4);  // Document
                            columns.RelativeColumn(2);  // Doc No
                            columns.RelativeColumn(1);  // Status
                            columns.RelativeColumn(3);  // Source/Target
                            columns.RelativeColumn(2);  // Amount
                            columns.RelativeColumn(2);  // Balance
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Date", "Time", "Document", "Doc No", "Status", "Source/Target", "Amount", "Balance"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(8);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.TransactionDate.ToString("dd/MM/yy"));
                            table.Cell().Padding(3).Text(line.TransactionTime?.ToString("HH:mm:ss") ?? "");
                            table.Cell().Padding(3).Text(line.DocumentTypeDescription);
                            table.Cell().Padding(3).Text(line.DocumentNumber);
                            table.Cell().Padding(3).Text(line.Status);
                            table.Cell().Padding(3).Text(line.SourceTarget);
                            table.Cell().Padding(3).AlignRight().Text($"{line.Amount:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.RunningBalance:N2}").Bold();
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().AlignRight().PaddingTop(4).Text($"C/F Balance to Next Month : {header.CarriedForwardBalance:N2}").Bold();
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
