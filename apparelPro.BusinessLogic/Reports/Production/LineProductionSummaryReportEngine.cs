using apparelPro.BusinessLogic.Services.Models.Production.ILineProductionSummaryReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_LPROD.PRG's print output - "LINE PRODUCTION SUMMARY".
    // Fixed 3-column layout (Line, This Period Qty, Cumulative Qty).
    public static class LineProductionSummaryReportEngine
    {
        public static byte[] GeneratePdf(LineProductionSummaryReportServiceModel report)
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
                        column.Item().AlignCenter().Text("[ LINE PRODUCTION SUMMARY ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4)
                            .Text($"From {report.StartDate:dd/MM/yyyy} to {report.EndDate:dd/MM/yyyy}  ·  Final section: {report.FinalSectionDescription}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.0f);  // Line
                            columns.RelativeColumn(1.5f);  // This Period Qty
                            columns.RelativeColumn(1.5f);  // Cumulative Qty
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Line").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("This Period Qty").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Cumulative Qty").Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Padding(2).Text(row.LineDescription);
                            table.Cell().Padding(2).AlignRight().Text($"{row.PeriodQty:N0}");
                            table.Cell().Padding(2).AlignRight().Text($"{row.CumulativeQty:N0}");
                        }

                        table.Cell().BorderTop(1.5f).Padding(3).Text("TOTAL").Bold();
                        table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{report.TotalPeriodQty:N0}").Bold();
                        table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{report.TotalCumulativeQty:N0}").Bold();
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
