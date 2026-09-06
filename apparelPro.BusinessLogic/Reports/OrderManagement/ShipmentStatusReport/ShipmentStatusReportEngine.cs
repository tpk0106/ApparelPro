using apparelPro.BusinessLogic.Services.Models.OrderManagement.IShipmentStatusReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.ShipmentStatusReport
{
    // Replicates OD_SHPST.PRG's "SHIPMENT STATUS REPORT" printed layout - each shipment
    // schedule row (Type/Style/Shp.Order No.) is followed by its actually-invoiced
    // quantities and a running "Balance to Ship" total, matching the legacy's nested
    // od_part -> ie_coin2 seek structure.
    public static class ShipmentStatusReportEngine
    {
        public static byte[] GeneratePdf(ShipmentStatusReportServiceModel report)
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
                            row.RelativeItem().Text("[ SHIPMENT STATUS REPORT ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span("Buyer: ").Bold(); t.Span($"{report.BuyerName}   ");
                            t.Span("Order: ").Bold(); t.Span(report.Order);
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(10);

                        foreach (var row in report.Rows)
                        {
                            column.Item().Column(rowGroup =>
                            {
                                rowGroup.Item().Background(Colors.Grey.Lighten4).Padding(4).Row(headerRow =>
                                {
                                    headerRow.RelativeItem(1f).Text(t => { t.Span("Type: ").SemiBold(); t.Span(row.TypeName); });
                                    headerRow.RelativeItem(2f).Text(t => { t.Span("Style: ").SemiBold(); t.Span(row.StyleCode); });
                                    headerRow.RelativeItem(2f).Text(t => { t.Span("Shp.Order No.: ").SemiBold(); t.Span(row.ShipmentOrderNo); });
                                    headerRow.RelativeItem(1f).Text(t => { t.Span("Unit: ").SemiBold(); t.Span(row.Unit); });
                                    headerRow.RelativeItem(2f).Text(t => { t.Span("Destination: ").SemiBold(); t.Span(row.DestinationCode); });
                                });

                                if (row.InvoiceLines.Count > 0)
                                {
                                    rowGroup.Item().PaddingLeft(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f); // Quantity Shipped
                                            columns.RelativeColumn(1f);   // Ship Date
                                            columns.RelativeColumn(2f);   // Invoice No
                                        });

                                        table.Header(tableHeader =>
                                        {
                                            foreach (var title in new[] { "Quantity Shipped", "Ship Date", "Invoice No" })
                                                tableHeader.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                        });

                                        foreach (var line in row.InvoiceLines)
                                        {
                                            table.Cell().Padding(2).AlignRight().Text($"{line.QuantityShipped:N2}");
                                            table.Cell().Padding(2).Text(line.InvoiceDate.HasValue ? $"{line.InvoiceDate:dd/MM/yy}" : "-");
                                            table.Cell().Padding(2).Text(line.InvoiceNumber);
                                        }
                                    });
                                }
                                else
                                {
                                    rowGroup.Item().PaddingLeft(10).Text("No invoiced shipments yet.").FontSize(7).Italic().FontColor(Colors.Grey.Darken1);
                                }

                                rowGroup.Item().PaddingLeft(10).PaddingTop(2).Row(balanceRow =>
                                {
                                    balanceRow.RelativeItem().Text("");
                                    balanceRow.ConstantItem(160).Text(t =>
                                    {
                                        t.Span("Balance to Ship: ").Bold();
                                        t.Span($"{row.BalanceToShip:N2}");
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
