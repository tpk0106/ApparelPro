using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderQuotaDetailReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.OrderQuotaDetailReport
{
    // Replicates OD_ROQ1.PRG's "ORDER QUOTA REPORT" printed layout.
    public static class OrderQuotaDetailReportEngine
    {
        public static byte[] GeneratePdf(OrderQuotaDetailReportServiceModel report)
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
                            row.RelativeItem().Text("[ ORDER QUOTA REPORT ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
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
                            columns.RelativeColumn(1.8f); // Order
                            columns.ConstantColumn(40);   // Type
                            columns.RelativeColumn(1.3f); // Style
                            columns.RelativeColumn(1.3f); // Shp Order No
                            columns.ConstantColumn(55);   // Quota Status
                            columns.RelativeColumn(1.2f); // Quota Year
                            columns.RelativeColumn(1.5f); // Quota Category
                            columns.ConstantColumn(60);   // Quota Type
                            columns.ConstantColumn(40);   // Unit
                            columns.RelativeColumn(1f);   // Qty
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[] { "Buyer", "Order", "Type", "Style", "Shp.Order No.", "Qta.Stat.", "Qta.Year", "Qta.Category", "Qta.Type", "Unit", "Quantity" })
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Padding(2).Text(row.BuyerName);
                            table.Cell().Padding(2).Text(row.Order);
                            table.Cell().Padding(2).Text(row.TypeName);
                            table.Cell().Padding(2).Text(row.StyleCode);
                            table.Cell().Padding(2).Text(row.ShipmentOrderNo);
                            table.Cell().Padding(2).Text(row.QuotaStatus == "Q" ? "Quota" : "Non-Quota");
                            table.Cell().Padding(2).Text($"{row.FromYearMonth} - {row.ToYearMonth}");
                            table.Cell().Padding(2).Text(row.QuotaCategory);
                            table.Cell().Padding(2).Text(row.QuotaType);
                            table.Cell().Padding(2).Text(row.Unit);
                            table.Cell().Padding(2).AlignRight().Text($"{row.Quantity:N2}");
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
