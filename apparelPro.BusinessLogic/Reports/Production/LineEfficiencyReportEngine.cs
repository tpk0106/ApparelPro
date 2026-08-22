using apparelPro.BusinessLogic.Services.Models.Production.ILineEfficiencyReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_GRH1.PRG's print output - "LINE EFFICIENCY" - as a table
    // with a compact text bar per day (the legacy ASCII chart's modern
    // equivalent; the interactive on-screen report renders a real bar chart).
    public static class LineEfficiencyReportEngine
    {
        public static byte[] GeneratePdf(LineEfficiencyReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(
                            $"[ LINE EFFICIENCY FOR {report.Year}-{report.Month:D2} ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text($"Line No.: {report.LineCode} - {report.LineDescription}");
                        column.Item().PaddingTop(2).Text(
                            $"Final section: {report.FinalSectionDescription}   Work Hours/Day: {report.WorkHoursPerDay}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f);  // Day
                            columns.RelativeColumn(1.2f);  // DOW
                            columns.RelativeColumn(1.2f);  // Efficiency
                            columns.RelativeColumn(4.0f);  // Bar
                            columns.RelativeColumn(2.5f);  // Note
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Day").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Day of Week").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Eff. %").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Note").Bold();
                        });

                        foreach (var cell in report.Days)
                        {
                            table.Cell().Padding(2).Text($"{cell.Day}");
                            table.Cell().Padding(2).Text(cell.DayOfWeek);
                            table.Cell().Padding(2).AlignRight().Text(cell.EfficiencyPercent.HasValue ? $"{cell.EfficiencyPercent:N0}" : "-");
                            var barLength = cell.EfficiencyPercent.HasValue
                                ? (int)Math.Min(cell.EfficiencyPercent.Value / 5m, 20)
                                : 0;
                            table.Cell().Padding(2).Text(new string('█', barLength));
                            table.Cell().Padding(2).Text(cell.IsHoliday ? cell.HolidayDescription ?? "Holiday" : "");
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().PaddingTop(4).Text(
                            $"Total Line Efficiency for Month : {report.MonthlyAverageEfficiencyPercent:N2}").Bold();
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
