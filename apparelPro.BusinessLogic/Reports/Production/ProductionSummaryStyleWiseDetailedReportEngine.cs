using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseDetailedReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Per-line companion to ProductionSummaryStyleWiseReportEngine - one
    // printed row per production Line instead of one per style, with the
    // style's Description/Unit Price/Basis/Value repeated on the line's
    // first row and blank on the rest (legacy's "-DO-" ditto convention,
    // rendered here as an empty cell instead of literal "-DO-" text).
    public static class ProductionSummaryStyleWiseDetailedReportEngine
    {
        public static byte[] GeneratePdf(ProductionSummaryStyleWiseDetailedReportServiceModel report)
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
                        column.Item().AlignCenter().Text("[ PRODUCTION SUMMARY - STYLE WISE (DETAILED) ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4)
                            .Text($"From {report.StartDate:dd/MM/yyyy} to {report.EndDate:dd/MM/yyyy}  ·  Value uses final section: {report.FinalSectionDescription}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.3f);  // Buyer
                            columns.RelativeColumn(1.8f);  // Order
                            columns.RelativeColumn(1.6f);  // Style
                            columns.RelativeColumn(2.4f);  // Description
                            columns.RelativeColumn(1.0f);  // Line
                            columns.RelativeColumn(1.6f);  // Order Qty
                            foreach (var _ in report.SectionCodes)
                                columns.RelativeColumn(1.3f); // Prod. Total Qty per section
                            columns.RelativeColumn(1.3f);  // Unit Price
                            columns.RelativeColumn(1.0f);  // Basis
                            columns.RelativeColumn(1.6f);  // Value
                        });

                        table.Header(headerRow =>
                        {
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Buyer").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Order").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Style").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Description").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Line").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Order Qty").Bold();
                            foreach (var sectionDescription in report.SectionDescriptions)
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text(t =>
                                {
                                    t.Line(sectionDescription).Bold();
                                    t.Line("Prod. Total Qty.").FontSize(6);
                                });
                            }
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Unit Price").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).Text("Basis").Bold();
                            headerRow.Cell().BorderBottom(1.5f).Padding(3).AlignRight().Text("Value").Bold();
                        });

                        foreach (var row in report.Rows)
                        {
                            var isFirstLine = true;
                            foreach (var line in row.Lines)
                            {
                                table.Cell().Padding(2).Text(isFirstLine ? row.BuyerName : "");
                                table.Cell().Padding(2).Text(isFirstLine ? row.Order : "");
                                table.Cell().Padding(2).Text(isFirstLine ? row.StyleCode : "");
                                table.Cell().Padding(2).Text(isFirstLine ? row.Description ?? "-" : "");
                                table.Cell().Padding(2).Text(line.LineCode);
                                table.Cell().Padding(2).AlignRight().Text($"{line.OrderQty:N0} {row.Unit}");

                                foreach (var sectionCode in report.SectionCodes)
                                {
                                    var quantity = line.SectionQuantities.FirstOrDefault(s => s.SectionCode == sectionCode)?.Quantity ?? 0;
                                    table.Cell().Padding(2).AlignRight().Text($"{quantity:N0}");
                                }

                                table.Cell().Padding(2).AlignRight().Text(isFirstLine ? $"{row.UnitPrice:N2}" : "");
                                table.Cell().Padding(2).Text(isFirstLine ? row.BasisCode ?? "-" : "");
                                table.Cell().Padding(2).AlignRight().Text(isFirstLine ? $"{row.Value:N2}" : "");

                                isFirstLine = false;
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
