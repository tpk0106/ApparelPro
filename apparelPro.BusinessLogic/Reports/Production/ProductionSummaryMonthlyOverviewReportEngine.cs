using System.Globalization;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyOverviewReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Print output for the simplified Monthly Production Summary companion
    // (not a legacy screen) - one row per day, one cell per line showing
    // today's Est/Act plus the running cumulative underneath.
    public static class ProductionSummaryMonthlyOverviewReportEngine
    {
        public static byte[] GeneratePdf(ProductionSummaryMonthlyOverviewReportServiceModel report)
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
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7));

                    var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(report.Month);

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ MONTHLY PRODUCTION SUMMARY - SIMPLIFIED ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4)
                            .Text($"For {monthName} {report.Year}  -  Final section: {report.FinalSectionDescription}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.6f); // Day
                            foreach (var _ in report.LineCodes) columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.6f); // Total
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Day").Bold();
                            foreach (var lineDesc in report.LineDescriptions)
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignCenter().Text(lineDesc).Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignCenter().Text("Total").Bold();
                        });

                        foreach (var day in report.Days)
                        {
                            table.Cell().Padding(2).Text($"{day.Date.Day}");

                            if (day.IsHoliday)
                            {
                                table.Cell().ColumnSpan((uint)(report.LineCodes.Count + 1)).Padding(2)
                                    .AlignCenter().Text(day.HolidayDescription ?? "").Italic();
                                continue;
                            }

                            foreach (var cell in day.LineCells)
                            {
                                table.Cell().Padding(2).AlignRight().Text(t =>
                                {
                                    t.Line($"{cell.EstQuantity:N0} / {cell.ActQuantity:N0}");
                                    t.Line($"Cum: {cell.CumEstQuantity:N0} / {cell.CumActQuantity:N0}").FontSize(6);
                                });
                            }

                            table.Cell().Padding(2).AlignRight().Text(t =>
                            {
                                t.Line($"{day.TotalEstQuantity:N0} / {day.TotalActQuantity:N0}").Bold();
                                t.Line($"Cum: {day.TotalCumEstQuantity:N0} / {day.TotalCumActQuantity:N0}").FontSize(6);
                            });
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
