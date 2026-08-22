using apparelPro.BusinessLogic.Services.Models.Production.IProductionScheduleReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_MSCHD.PRG's print output - "PRODUCTION SCHEDULE".
    public static class ProductionScheduleReportEngine
    {
        public static byte[] GeneratePdf(ProductionScheduleReportServiceModel report)
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
                        column.Item().AlignCenter().Text("[ PRODUCTION SCHEDULE ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4)
                            .Text($"From {report.FromDate:dd/MM/yyyy} to {report.ToDate:dd/MM/yyyy}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f);  // Line
                            columns.RelativeColumn(1.3f);  // Est. Start
                            columns.RelativeColumn(1.3f);  // Est. End
                            columns.RelativeColumn(1.8f);  // Buyer
                            columns.RelativeColumn(1.6f);  // Order
                            columns.RelativeColumn(1.6f);  // Style
                            columns.RelativeColumn(1.6f);  // Shipment
                            columns.RelativeColumn(1.2f);  // Est. Prod/Day
                            columns.RelativeColumn(0.9f);  // Unit
                            columns.RelativeColumn(1.0f);  // Lead Time
                            columns.RelativeColumn(1.0f);  // No. Days
                            columns.RelativeColumn(1.3f);  // Total Qty
                            columns.RelativeColumn(1.3f);  // Ship Date
                            columns.RelativeColumn(0.9f);  // Float
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Line", "Est. Start", "Est. End", "Buyer", "Order", "Style", "Shipment",
                                "Est. Prod/Day", "Unit", "Lead Time", "No. Days", "Total Qty", "Ship Date", "Float"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold();
                            }
                        });

                        foreach (var line in report.Lines)
                        {
                            table.Cell().Padding(2).Text(line.LineCode);
                            table.Cell().Padding(2).Text($"{line.EstimatedStartDate:dd/MM/yy}");
                            table.Cell().Padding(2).Text($"{line.EstimatedEndDate:dd/MM/yy}");
                            table.Cell().Padding(2).Text(line.BuyerName);
                            table.Cell().Padding(2).Text(line.Order);
                            table.Cell().Padding(2).Text(line.StyleCode);
                            table.Cell().Padding(2).Text(line.ShipmentOrder);
                            table.Cell().Padding(2).AlignRight().Text($"{line.EstimatedProductionPerDay:N0}");
                            table.Cell().Padding(2).Text(line.Unit);
                            table.Cell().Padding(2).AlignRight().Text($"{line.LeadTimeDays:N1}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.NumberOfDays:N1}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.TotalQuantity:N0}");
                            table.Cell().Padding(2).Text(line.ShipDate.HasValue ? $"{line.ShipDate:dd/MM/yy}" : "-");

                            var floatCell = table.Cell().Padding(2).AlignRight();
                            if (line.FloatDays is int floatDays)
                                floatCell.Text($"{floatDays}").FontColor(floatDays < 0 ? "#D03B3B" : "#0CA30C");
                            else
                                floatCell.Text("-");
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
