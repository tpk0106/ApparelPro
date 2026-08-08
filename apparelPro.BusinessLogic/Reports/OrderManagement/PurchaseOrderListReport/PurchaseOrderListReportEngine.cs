using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderListReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.PurchaseOrderListReport
{
    // Replicates OD_POLST.PRG's "PURCHASE ORDER LIST" printed layout: one flat table of
    // every line item on the given Supplier Purchase Order, each cross-referenced back
    // to the Buyer/Order/Type/Style it was raised against. Landscape orientation - the
    // legacy print statements position fields out to column 133 despite the nominal
    // "[80]" width parameter passed to scr_prn, so this follows the wider actual layout.
    public static class PurchaseOrderListReportEngine
    {
        public static byte[] GeneratePdf(PurchaseOrderListReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            QuestPDF.Fluent.Document.Create(container =>
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
                            row.RelativeItem().Text("[ P U R C H A S E   O R D E R   L I S T ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("P/O No: ").Bold(); t.Span(report.PurchaseOrderNumber); });
                            row.RelativeItem().Text(t => { t.Span("Supplier: ").Bold(); t.Span(string.IsNullOrEmpty(report.SupplierName) ? report.SupplierCode : report.SupplierName); });
                            row.RelativeItem().Text(t => { t.Span("Currency: ").Bold(); t.Span(report.CurrencyCode); });
                        });
                        column.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("P/I No: ").Bold(); t.Span(report.ProformaInvoiceNo ?? "-"); });
                            row.RelativeItem().Text(t => { t.Span("P/I Date: ").Bold(); t.Span(report.ProformaInvoiceDate.HasValue ? report.ProformaInvoiceDate.Value.ToString("dd-MMM-yyyy") : "-"); });
                            row.RelativeItem();
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.6f); // Item Code
                                columns.RelativeColumn(2.6f); // Description
                                columns.ConstantColumn(60);   // Order Qty
                                columns.ConstantColumn(45);   // Unit
                                columns.ConstantColumn(60);   // Unit Price
                                columns.RelativeColumn(1.4f); // Buyer
                                columns.RelativeColumn(1.2f); // Order
                                columns.RelativeColumn(1.4f); // Type
                                columns.RelativeColumn(1.2f); // Style
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item Code").Bold().FontSize(7);
                                header.Cell().Text("Description").Bold().FontSize(7);
                                header.Cell().PaddingRight(6).AlignRight().Text("Order Qty").Bold().FontSize(7);
                                header.Cell().Text("Unit").Bold().FontSize(7);
                                header.Cell().PaddingRight(6).AlignRight().Text("Unit Price").Bold().FontSize(7);
                                header.Cell().Text("Buyer").Bold().FontSize(7);
                                header.Cell().Text("Order").Bold().FontSize(7);
                                header.Cell().Text("Type").Bold().FontSize(7);
                                header.Cell().Text("Style").Bold().FontSize(7);
                                header.Cell().ColumnSpan(9).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                            });

                            foreach (var line in report.Lines)
                            {
                                table.Cell().PaddingVertical(2).Text(line.ItemCode).FontSize(7);
                                table.Cell().PaddingVertical(2).Text(line.Description).FontSize(7);
                                table.Cell().PaddingVertical(2).PaddingRight(6).AlignRight().Text(line.OrderQuantity.ToString("#,##0.##")).FontSize(7);
                                table.Cell().PaddingVertical(2).Text(line.OrderUnit).FontSize(7);
                                table.Cell().PaddingVertical(2).PaddingRight(6).AlignRight().Text(line.UnitPrice.ToString("#,##0.00")).FontSize(7);
                                table.Cell().PaddingVertical(2).Text(string.IsNullOrEmpty(line.BuyerName) ? line.BuyerCode.ToString() : line.BuyerName).FontSize(7);
                                table.Cell().PaddingVertical(2).Text(line.Order).FontSize(7);
                                table.Cell().PaddingVertical(2).Text(string.IsNullOrEmpty(line.TypeName) ? line.TypeCode.ToString() : line.TypeName).FontSize(7);
                                table.Cell().PaddingVertical(2).Text(line.StyleCode).FontSize(7);
                            }
                        });
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("[ End of Report ]").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.DefaultTextStyle(TextStyle.Default.FontSize(8));
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
