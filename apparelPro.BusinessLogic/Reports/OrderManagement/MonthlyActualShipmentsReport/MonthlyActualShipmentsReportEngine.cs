using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMonthlyActualShipmentsReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.MonthlyActualShipmentsReport
{
    // Replicates OD_ACTSP.PRG's "MONTHLY ACTUAL SHIPMENTS" printed layout.
    public static class MonthlyActualShipmentsReportEngine
    {
        public static byte[] GeneratePdf(MonthlyActualShipmentsReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"[ SHIPMENT FOR MONTH OF {report.Month:00}/{report.Year} ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.3f); // Invoice
                            columns.RelativeColumn(1.5f); // Buyer
                            columns.RelativeColumn(1.8f); // Order No
                            columns.RelativeColumn(1.3f); // Style
                            columns.ConstantColumn(55);   // Date
                            columns.RelativeColumn(1f);   // Qty
                            columns.RelativeColumn(1f);   // Balance
                            columns.RelativeColumn(1.2f); // Value
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[] { "Invoice No.", "Buyer", "Order No", "Style", "Date", "Quantity", "Balance", "Value" })
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Padding(2).Text(row.InvoiceNumber);
                            table.Cell().Padding(2).Text(row.BuyerName);
                            table.Cell().Padding(2).Text(row.OrderNo);
                            table.Cell().Padding(2).Text(row.StyleCode);
                            table.Cell().Padding(2).Text($"{row.ShipDate:dd/MM/yy}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.Quantity:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.Balance:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.Value:N2}");
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
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
