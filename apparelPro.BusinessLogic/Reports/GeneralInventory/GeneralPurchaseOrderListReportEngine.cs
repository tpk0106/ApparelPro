using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_PLIST.PRG's printed report.
    public static class GeneralPurchaseOrderListReportEngine
    {
        public static byte[] GeneratePurchaseOrderListReportPdf(
            GeneralPurchaseOrderListReportHeaderServiceModel header,
            List<GeneralPurchaseOrderListReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

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
                        column.Item().AlignCenter().Text("List of P/O's (General)").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"From {header.FromDate:dd/MM/yyyy} to {header.ToDate:dd/MM/yyyy}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // P/O No
                            columns.RelativeColumn(2);  // Date
                            columns.RelativeColumn(2);  // Time
                            columns.RelativeColumn(5);  // Supplier
                            columns.RelativeColumn(1);  // Basis
                            columns.RelativeColumn(2);  // PI No
                            columns.RelativeColumn(1);  // Curr
                            columns.RelativeColumn(3);  // Prepared By
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "P/O No", "Date", "Time", "Supplier", "Basis", "PI No", "Curr", "Prepared By"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(8);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.PoNumber);
                            table.Cell().Padding(3).Text(line.OrderDate?.ToString("dd/MM/yy") ?? "");
                            table.Cell().Padding(3).Text(line.OrderTime?.ToString("HH:mm:ss") ?? "");
                            table.Cell().Padding(3).Text(line.SupplierName);
                            table.Cell().Padding(3).Text(line.BasisCode ?? "");
                            table.Cell().Padding(3).Text(line.ProformaInvoiceNo ?? "");
                            table.Cell().Padding(3).Text(line.CurrencyCode ?? "");
                            table.Cell().Padding(3).Text(line.PreparedBy);
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
