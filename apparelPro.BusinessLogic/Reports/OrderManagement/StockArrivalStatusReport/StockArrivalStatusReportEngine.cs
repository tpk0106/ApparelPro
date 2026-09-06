using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStockArrivalStatusReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.StockArrivalStatusReport
{
    // Replicates OD_STARV.PRG's "STOCK ARRIVAL STATUS REPORT" printed layout - one
    // section per budgeted material item, listing every PO line raised to cover it.
    public static class StockArrivalStatusReportEngine
    {
        public static byte[] GeneratePdf(StockArrivalStatusReportServiceModel report)
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
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ STOCK ARRIVAL STATUS REPORT ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span("Buyer: ").Bold(); t.Span($"{report.BuyerName}    ");
                            t.Span("Order: ").Bold(); t.Span($"{report.Order}    ");
                            t.Span("Report Date: ").Bold(); t.Span($"{report.AsOfDate:dd-MMM-yy}    ");
                            t.Span("Total Order Qty: ").Bold(); t.Span($"{report.TotalOrderQuantity:N2} {report.Unit}");
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(10);

                        foreach (var item in report.Items)
                        {
                            column.Item().Column(itemCol =>
                            {
                                itemCol.Item().Background(Colors.Grey.Lighten4).Padding(4).Row(headerRow =>
                                {
                                    headerRow.RelativeItem(1.5f).Text(t => { t.Span("Item: ").SemiBold(); t.Span($"{item.ItemCode}    "); t.Span(item.Description); });
                                    headerRow.RelativeItem(1f).Text(t => { t.Span("Order Qty: ").SemiBold(); t.Span($"{item.OrderedQuantity:N2} {item.Unit}"); });
                                    headerRow.RelativeItem(1f).Text(t => { t.Span("Received: ").SemiBold(); t.Span($"{item.TotalReceivedQuantity:N2}"); });
                                    headerRow.RelativeItem(1f).Text(t => { t.Span("Balance: ").SemiBold(); t.Span($"{item.BalanceToReceive:N2}"); });
                                });

                                if (item.PurchaseOrderLines.Count > 0)
                                {
                                    itemCol.Item().PaddingLeft(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(60);   // PO No
                                            columns.RelativeColumn(1f);   // PO Qty
                                            columns.ConstantColumn(50);   // Store
                                            columns.RelativeColumn(2f);   // Supplier
                                            columns.ConstantColumn(70);   // Expected Date
                                            columns.ConstantColumn(50);   // Delay
                                            columns.RelativeColumn(1f);   // Supplier Return
                                        });

                                        table.Header(header =>
                                        {
                                            foreach (var title in new[] { "P/O No", "P/O Qty", "Store", "Supplier", "Expected Date", "Delay", "Supp. Return" })
                                                header.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                        });

                                        foreach (var line in item.PurchaseOrderLines)
                                        {
                                            table.Cell().Padding(2).Text(line.PurchaseOrderNumber);
                                            table.Cell().Padding(2).AlignRight().Text($"{line.OrderedQuantity:N2}");
                                            table.Cell().Padding(2).Text(line.StoreCode);
                                            table.Cell().Padding(2).Text(line.SupplierName);
                                            table.Cell().Padding(2).Text(line.ExpectedDate.HasValue ? $"{line.ExpectedDate:dd-MMM-yy}" : "Not Specified");
                                            table.Cell().Padding(2).Text(line.DelayDays.HasValue ? $"{line.DelayDays}" : "");
                                            table.Cell().Padding(2).AlignRight().Text($"{line.SupplierReturnQuantity:N2}");
                                        }
                                    });
                                }
                                else
                                {
                                    itemCol.Item().PaddingLeft(10).Text("** Purchase Order Not Raised **").FontSize(8).Italic().FontColor(Colors.Red.Darken1);
                                }
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
