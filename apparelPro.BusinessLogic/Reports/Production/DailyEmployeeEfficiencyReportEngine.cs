using apparelPro.BusinessLogic.Services.Models.Production.IDailyEmployeeEfficiencyReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_EEF1.PRG's print output - "DAILY EMPLOYEE EFFICIENCY".
    public static class DailyEmployeeEfficiencyReportEngine
    {
        public static byte[] GeneratePdf(DailyEmployeeEfficiencyReportServiceModel report)
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
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ DAILY EMPLOYEE EFFICIENCY ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text(
                            $"Date: {report.Date:dd/MM/yyyy}" +
                            (report.LineCode != null ? $"   Line: {report.LineCode} - {report.LineDescription}" : ""));
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.0f);  // Emp#
                            columns.RelativeColumn(2.5f);  // Name
                            columns.RelativeColumn(1.2f);  // Hrs Worked
                            columns.RelativeColumn(1.2f);  // Hrs Earned
                            columns.RelativeColumn(1.0f);  // NP Hrs
                            columns.RelativeColumn(1.4f);  // Overall Eff
                            columns.RelativeColumn(1.4f);  // Operator Eff
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Emp. #").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Employee Name").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Hrs. Worked").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Hrs. Earned").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("NP Hrs.").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Overall Eff.").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Operator Eff.").Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Padding(2).Text(row.EmployeeCode);
                            table.Cell().Padding(2).Text(row.EmployeeName);
                            table.Cell().Padding(2).AlignRight().Text($"{row.WorkHours:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.EarnedHours:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.NonProductiveHours:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.OverEfficiencyPercent:N2} %");
                            table.Cell().Padding(2).AlignRight().Text($"{row.OperatorEfficiencyPercent:N2} %");
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
