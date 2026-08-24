using apparelPro.BusinessLogic.Services.Models.OrderManagement.IScheduledShipmentsReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.ScheduledShipmentsReport
{
    // Replicates OD_RSHP1.PRG's "SCHEDULE SHIPMENT DETAIL REPORT" printed layout.
    public static class ScheduledShipmentsReportEngine
    {
        public static byte[] GeneratePdf(ScheduledShipmentsReportServiceModel report)
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
                            row.RelativeItem().Text("[ SCHEDULE SHIPMENT DETAIL REPORT ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        if (report.BuyerCode.HasValue || !string.IsNullOrEmpty(report.Order))
                        {
                            column.Item().PaddingTop(6).Text(t =>
                            {
                                if (report.BuyerCode.HasValue) { t.Span("Buyer: ").Bold(); t.Span(report.BuyerCode.Value.ToString() + "   "); }
                                if (!string.IsNullOrEmpty(report.Order)) { t.Span("Order: ").Bold(); t.Span(report.Order); }
                            });
                        }
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);   // Buyer
                            columns.RelativeColumn(2f);   // Order
                            columns.ConstantColumn(40);   // Type
                            columns.RelativeColumn(1.5f); // Style
                            columns.RelativeColumn(1.5f); // Shp Order No
                            columns.ConstantColumn(40);   // Unit
                            columns.RelativeColumn(1f);   // Qty
                            columns.RelativeColumn(1.5f); // Destination
                            columns.ConstantColumn(60);   // Ship Date
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[] { "Buyer", "Order", "Type", "Style", "Shp.Order No.", "Unit", "Quantity", "Destination", "Ship Date" })
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Padding(2).Text(row.BuyerName);
                            table.Cell().Padding(2).Text(row.Order);
                            table.Cell().Padding(2).Text(row.TypeName);
                            table.Cell().Padding(2).Text(row.StyleCode);
                            table.Cell().Padding(2).Text(row.ShipmentOrderNo);
                            table.Cell().Padding(2).Text(row.Unit);
                            table.Cell().Padding(2).AlignRight().Text($"{row.Quantity:N2}");
                            table.Cell().Padding(2).Text(row.DestinationCode);
                            table.Cell().Padding(2).Text($"{row.ShipDate:dd/MM/yy}");
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
