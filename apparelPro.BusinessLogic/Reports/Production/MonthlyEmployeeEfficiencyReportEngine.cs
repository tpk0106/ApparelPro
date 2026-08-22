using apparelPro.BusinessLogic.Services.Models.Production.IMonthlyEmployeeEfficiencyReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_REP4.PRG's print output - "EMPLOYEE EFFICIENCY REPORT".
    // Wide dynamic grid (one column per day of the month).
    public static class MonthlyEmployeeEfficiencyReportEngine
    {
        public static byte[] GeneratePdf(MonthlyEmployeeEfficiencyReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text($"[ EMPLOYEE EFFICIENCY REPORT FOR {report.Year}-{report.Month:D2} ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text($"Work Hours/Day: {report.WorkHoursPerDay} (factory-wide)");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.0f);  // Emp#
                            columns.RelativeColumn(2.5f);  // Name
                            for (var i = 0; i < report.DaysInMonth; i++)
                                columns.RelativeColumn(0.6f);
                            columns.RelativeColumn(1.0f);  // Monthly Avg
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Emp #").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Employee Name").Bold();
                            for (var day = 1; day <= report.DaysInMonth; day++)
                                headerRow.Cell().BorderBottom(1.5f).Padding(1).AlignCenter().Text($"{day}").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignCenter().Text("Avg").Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Padding(2).Text(row.EmployeeCode);
                            table.Cell().Padding(2).Text(row.EmployeeName);
                            foreach (var cell in row.Days)
                            {
                                table.Cell().Padding(1).AlignCenter().Text(
                                    cell.OperatorEfficiencyPercent.HasValue ? $"{cell.OperatorEfficiencyPercent:N1}" : "");
                            }
                            table.Cell().Padding(2).AlignCenter().Text($"{row.MonthlyAverageEfficiencyPercent:N1}").Bold();
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
