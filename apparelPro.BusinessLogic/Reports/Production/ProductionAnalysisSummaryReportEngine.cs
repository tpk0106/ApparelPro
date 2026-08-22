using apparelPro.BusinessLogic.Services.Models.Production.IProductionAnalysisSummaryReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_MPRO2.PRG's print output - "PRODUCTION ANALYSIS SUMMARY
    // - (for style)". Columns are dynamic (2 fixed + 1 per Section), each
    // cell aligned to its actual Section code (see the service model's
    // comment on the deliberate column-alignment fix vs. legacy).
    public static class ProductionAnalysisSummaryReportEngine
    {
        public static byte[] GeneratePdf(ProductionAnalysisSummaryReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ PRODUCTION ANALYSIS SUMMARY - (For Style) ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text(
                            $"Buyer: {report.BuyerName}   Order: {report.Order}   Type: {report.TypeName}   Style: {report.StyleCode}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.4f);  // Date
                                columns.RelativeColumn(1.0f);  // Line
                                foreach (var _ in report.SectionCodes)
                                    columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.2f);  // Total
                            });

                            table.Header(headerRow =>
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Date").Bold();
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Line").Bold();
                                foreach (var sectionDescription in report.SectionDescriptions)
                                    headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text(sectionDescription).Bold();
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Total").Bold();
                            });

                            DateOnly? lastDate = null;
                            foreach (var row in report.Rows)
                            {
                                table.Cell().Padding(2).Text(lastDate == row.Date ? "" : $"{row.Date:dd/MM/yyyy}");
                                lastDate = row.Date;
                                table.Cell().Padding(2).Text(row.LineCode);
                                foreach (var sectionCode in report.SectionCodes)
                                {
                                    var qty = row.SectionQuantities.FirstOrDefault(s => s.SectionCode == sectionCode)?.Quantity ?? 0;
                                    table.Cell().Padding(2).AlignRight().Text($"{qty:N0}");
                                }
                                table.Cell().Padding(2).AlignRight().Text($"{row.Total:N0}");
                            }

                            table.Cell().ColumnSpan(2).BorderTop(1.5f).Padding(3).Text("TOTAL").Bold();
                            decimal grandTotal = 0;
                            foreach (var sectionCode in report.SectionCodes)
                            {
                                var qty = report.SectionTotals.FirstOrDefault(s => s.SectionCode == sectionCode)?.Quantity ?? 0;
                                grandTotal += qty;
                                table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{qty:N0}").Bold();
                            }
                            table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{grandTotal:N0}").Bold();
                        });

                        column.Item().PaddingTop(12).Text(
                            $"AVERAGE PRODUCTION QUANTITY ON FINAL OUTPUT - {report.FinalSectionDescription} : " +
                            $"{report.AverageProductionQuantityOnFinalOutput:N2}  [{report.FinalOutputProductionDays} day(s)]");
                        column.Item().PaddingTop(4).Text(
                            $"Total No of days taken for Production : {report.TotalDaysTakenForProduction}");
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
