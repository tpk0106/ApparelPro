using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_MPRO1.PRG's print output - "PRODUCTION SUMMARY - STYLE
    // WISE". Columns are dynamic (7 fixed + 1 per Section), same as the
    // on-screen grid.
    public static class ProductionSummaryStyleWiseReportEngine
    {
        public static byte[] GeneratePdf(ProductionSummaryStyleWiseReportServiceModel report)
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
                        column.Item().AlignCenter().Text("[ PRODUCTION SUMMARY - STYLE WISE ]").FontSize(13).Bold();
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
                            columns.RelativeColumn(2.6f);  // Description
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
                            table.Cell().Padding(2).Text(row.BuyerName);
                            table.Cell().Padding(2).Text(row.Order);
                            table.Cell().Padding(2).Text(row.StyleCode);
                            table.Cell().Padding(2).Text(row.Description ?? "-");
                            table.Cell().Padding(2).AlignRight().Text($"{row.OrderQty:N0} {row.Unit}");

                            foreach (var sectionCode in report.SectionCodes)
                            {
                                var quantity = row.SectionQuantities.FirstOrDefault(s => s.SectionCode == sectionCode)?.Quantity ?? 0;
                                table.Cell().Padding(2).AlignRight().Text($"{quantity:N0}");
                            }

                            table.Cell().Padding(2).AlignRight().Text($"{row.UnitPrice:N2}");
                            table.Cell().Padding(2).Text(row.BasisCode ?? "-");
                            table.Cell().Padding(2).AlignRight().Text($"{row.Value:N2}");
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
