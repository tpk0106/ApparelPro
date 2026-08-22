using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryDailyReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_DPROD.PRG's print output - "DAILY PRODUCTION SUMMARY".
    // Columns are dynamic (6 fixed + 3 per Section), same as the on-screen
    // grid, so the column count/widths are computed from the report data
    // rather than hardcoded like the fixed-shape reports elsewhere.
    public static class ProductionSummaryDailyReportEngine
    {
        public static byte[] GeneratePdf(ProductionSummaryDailyReportServiceModel report)
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
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ DAILY PRODUCTION SUMMARY ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4)
                            .Text($"Date : {report.Date:dd/MM/yyyy}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.3f);  // Buyer
                            columns.RelativeColumn(1.8f);  // Order
                            columns.RelativeColumn(1.8f);  // Style
                            columns.RelativeColumn(2.4f);  // Description
                            columns.RelativeColumn(1.6f);  // Order Qty
                            columns.RelativeColumn(1.0f);  // Line
                            foreach (var _ in report.SectionCodes)
                            {
                                columns.RelativeColumn(1.1f); // Pro Qty
                                columns.RelativeColumn(1.1f); // To-Date
                                columns.RelativeColumn(1.1f); // Balance
                            }
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Buyer").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Order").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Style").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Description").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Order Qty").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Line").Bold();
                            foreach (var sectionDescription in report.SectionDescriptions)
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text(t =>
                                {
                                    t.Line(sectionDescription).Bold();
                                    t.Line("Pro Qty").FontSize(6);
                                });
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text(t =>
                                {
                                    t.Line(sectionDescription).Bold();
                                    t.Line("To-Date").FontSize(6);
                                });
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text(t =>
                                {
                                    t.Line(sectionDescription).Bold();
                                    t.Line("Balance").FontSize(6);
                                });
                            }
                        });

                        foreach (var line in report.Lines)
                        {
                            table.Cell().Padding(2).Text(line.BuyerName);
                            table.Cell().Padding(2).Text(line.Order);
                            table.Cell().Padding(2).Text(line.StyleCode);
                            table.Cell().Padding(2).Text(line.Description ?? "-");
                            table.Cell().Padding(2).AlignRight().Text($"{line.OrderQuantity:N0} {line.Unit}");
                            table.Cell().Padding(2).Text(line.LineCode);

                            foreach (var section in line.Sections)
                            {
                                table.Cell().Padding(2).AlignRight().Text($"{section.ProQuantity:N0}");
                                table.Cell().Padding(2).AlignRight().Text($"{section.ToDateQuantity:N0}");
                                table.Cell().Padding(2).AlignRight().Text($"{section.Balance:N0}");
                            }
                        }

                        // Totals row - see ProductionSummaryDailyReportServiceModel's
                        // comment for why Balance here isn't a sum of the line balances.
                        table.Cell().ColumnSpan(4).BorderTop(1.5f).Padding(3).Text("T O T A L").Bold();
                        table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{report.TotalOrderQuantity:N0}").Bold();
                        table.Cell().BorderTop(1.5f).Padding(3).Text("");
                        foreach (var total in report.Totals)
                        {
                            table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{total.ProQuantity:N0}").Bold();
                            table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{total.ToDateQuantity:N0}").Bold();
                            table.Cell().BorderTop(1.5f).Padding(3).AlignRight().Text($"{total.Balance:N0}").Bold();
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
