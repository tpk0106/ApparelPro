using apparelPro.BusinessLogic.Services.Models.OrderManagement.IYearSeasonOrdersReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.YearSeasonOrdersReport
{
    // Replicates OD_RPO2.PRG's "ORDER CONFIRMATION REPORT" printed layout - each order is
    // followed by its Style lines and a grand total value, matching the legacy's nested
    // od_po -> od_style seek structure.
    public static class YearSeasonOrdersReportEngine
    {
        public static byte[] GeneratePdf(YearSeasonOrdersReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ YEAR/SEASON WISE ORDERS ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        if (report.Year.HasValue || !string.IsNullOrEmpty(report.Season))
                        {
                            column.Item().PaddingTop(6).Text(t =>
                            {
                                if (report.Year.HasValue) { t.Span("Year: ").Bold(); t.Span(report.Year.Value.ToString() + "   "); }
                                if (!string.IsNullOrEmpty(report.Season)) { t.Span("Season: ").Bold(); t.Span(report.Season); }
                            });
                        }
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(10);

                        foreach (var row in report.Rows)
                        {
                            column.Item().Column(rowGroup =>
                            {
                                rowGroup.Item().Background(Colors.Grey.Lighten4).Padding(4).Column(headerCol =>
                                {
                                    headerCol.Item().Row(r =>
                                    {
                                        r.RelativeItem(1.2f).Text(t => { t.Span("Buyer: ").SemiBold(); t.Span($"{row.BuyerName} ({row.BuyerCode})"); });
                                        r.RelativeItem(1f).Text(t => { t.Span("Order: ").SemiBold(); t.Span(row.Order); });
                                        r.RelativeItem(1f).Text(t => { t.Span("Date: ").SemiBold(); t.Span($"{row.OrderDate:dd/MM/yyyy}"); });
                                        r.RelativeItem(1f).Text(t => { t.Span("Season: ").SemiBold(); t.Span(row.SeasonDescription); });
                                    });
                                    headerCol.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.RelativeItem(2f).Text(t => { t.Span("Description: ").SemiBold(); t.Span(row.Description ?? "-"); });
                                        r.RelativeItem(1f).Text(t => { t.Span("Country: ").SemiBold(); t.Span(row.CountryCode); });
                                        r.RelativeItem(1f).Text(t => { t.Span("Qty: ").SemiBold(); t.Span($"{row.TotalQuantity:N2} {row.Unit}"); });
                                        r.RelativeItem(1f).Text(t => { t.Span("Currency: ").SemiBold(); t.Span(row.CurrencyCode); });
                                    });
                                });

                                if (row.Styles.Count > 0)
                                {
                                    rowGroup.Item().PaddingLeft(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(40);   // Type
                                            columns.RelativeColumn(1.5f); // Style
                                            columns.ConstantColumn(40);   // Unit
                                            columns.RelativeColumn(1f);   // Qty
                                            columns.RelativeColumn(1f);   // Unit Price
                                            columns.RelativeColumn(1.2f); // Total Value
                                        });

                                        table.Header(tableHeader =>
                                        {
                                            foreach (var title in new[] { "Type", "Style", "Unit", "Quantity", "Unit Price", "Total Value" })
                                                tableHeader.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                        });

                                        foreach (var style in row.Styles)
                                        {
                                            table.Cell().Padding(2).Text($"{style.TypeCode}");
                                            table.Cell().Padding(2).Text(style.StyleCode);
                                            table.Cell().Padding(2).Text(style.Unit);
                                            table.Cell().Padding(2).AlignRight().Text($"{style.Quantity:N2}");
                                            table.Cell().Padding(2).AlignRight().Text($"{style.UnitPrice:N2}");
                                            table.Cell().Padding(2).AlignRight().Text($"{style.TotalValue:N2}");
                                        }
                                    });
                                }

                                rowGroup.Item().PaddingLeft(10).PaddingTop(2).Row(totalRow =>
                                {
                                    totalRow.RelativeItem().Text("");
                                    totalRow.ConstantItem(180).Text(t =>
                                    {
                                        t.Span("Grand Total Value: ").Bold();
                                        t.Span($"{row.GrandTotalValue:N2}");
                                    });
                                });
                            });
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
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
