using System.Globalization;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_MPROD.PRG's print output - "MONTHLY PRODUCTION SUMMARY".
    // A day with a style changeover mid-line prints as multiple stacked
    // rows (see ProductionSummaryMonthlyReportServiceModel) - the day number
    // is only printed on the first of those rows, same as the legacy report.
    public static class ProductionSummaryMonthlyReportEngine
    {
        public static byte[] GeneratePdf(ProductionSummaryMonthlyReportServiceModel report)
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
                        column.Item().AlignCenter().Text("[ MONTHLY PRODUCTION SUMMARY ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4)
                            .Text($"For {monthName} {report.Year}  -  Final section: {report.FinalSectionDescription}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.6f); // Day
                            foreach (var _ in report.LineCodes) columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.8f); // Total
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
                            if (day.IsHoliday)
                            {
                                table.Cell().Padding(2).Text($"{day.Date.Day}");
                                table.Cell().ColumnSpan((uint)(report.LineCodes.Count + 1)).Padding(2)
                                    .AlignCenter().Text(day.HolidayDescription ?? "").Italic();
                                continue;
                            }

                            for (var subRowIndex = 0; subRowIndex < day.SubRows.Count; subRowIndex++)
                            {
                                var subRow = day.SubRows[subRowIndex];
                                table.Cell().Padding(2).Text(subRowIndex == 0 ? $"{day.Date.Day}" : "");

                                foreach (var cell in subRow.LineCells)
                                {
                                    table.Cell().Padding(2).AlignRight().Text(t =>
                                    {
                                        t.Line(cell.StyleCode ?? "-").FontSize(6);
                                        t.Line($"{cell.EstQuantity?.ToString("N0") ?? "-"} / {cell.ActQuantity?.ToString("N0") ?? "-"}");
                                        t.Line($"Cum: {cell.CumEstQuantity:N0} / {cell.CumActQuantity:N0}").FontSize(6);
                                    });
                                }

                                table.Cell().Padding(2).AlignRight().Text(t =>
                                {
                                    t.Line($"{subRow.TotalEstQuantity:N0} / {subRow.TotalActQuantity:N0}").Bold();
                                    t.Line($"Cum: {subRow.TotalCumEstQuantity:N0} / {subRow.TotalCumActQuantity:N0}").FontSize(6);
                                });
                            }
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
