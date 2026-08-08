using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOutstandingPurchaseOrderListReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.OutstandingPurchaseOrderListReport
{
    // Replicates OD_PLST1.PRG's "LIST OF OUTSTANDING P/O's - Date Wise" printed layout:
    // one section per Basis (name shown, not code, per the project's naming convention),
    // each with a flat table listing every outstanding Buyer/Order/Type/Style group
    // found on every qualifying P/O (see OutstandingPurchaseOrderListReportServiceModel
    // for the corrected "check every group" logic). Landscape orientation, matching
    // legacy's 132-column-wide print job (do scr_prn with [80] parameter is followed by
    // print statements positioned out past column 132, same reasoning as the Purchase
    // Order List Report and Colour/Size Report).
    public static class OutstandingPurchaseOrderListReportEngine
    {
        public static byte[] GeneratePdf(OutstandingPurchaseOrderListReportServiceModel report)
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
                            row.RelativeItem().Text("[ OUTSTANDING P/O's LISTING - Date Wise ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Start Date: ").Bold(); t.Span(report.StartDate.ToString("dd-MMM-yyyy")); });
                            row.RelativeItem().Text(t => { t.Span("End Date: ").Bold(); t.Span(report.EndDate.ToString("dd-MMM-yyyy")); });
                            row.RelativeItem().Text(t => { t.Span("Basis: ").Bold(); t.Span(string.IsNullOrEmpty(report.BasisCode) ? "(All)" : report.BasisCode); });
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        foreach (var basisGroup in report.BasisGroups)
                        {
                            column.Item().PaddingTop(6).Text(string.IsNullOrEmpty(basisGroup.BasisName) ? basisGroup.BasisCode : basisGroup.BasisName)
                                .Bold().FontSize(9).FontColor(Colors.Blue.Darken3);

                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.3f); // P/O No
                                    columns.RelativeColumn(1.0f); // Date
                                    columns.RelativeColumn(2.2f); // Supplier
                                    columns.RelativeColumn(1.3f); // P/I No
                                    columns.RelativeColumn(0.8f); // Currency
                                    columns.RelativeColumn(1.4f); // Buyer
                                    columns.RelativeColumn(1.2f); // Order
                                    columns.RelativeColumn(1.4f); // Type
                                    columns.RelativeColumn(1.2f); // Style
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("P/O No").Bold().FontSize(7);
                                    header.Cell().Text("Date").Bold().FontSize(7);
                                    header.Cell().Text("Supplier").Bold().FontSize(7);
                                    header.Cell().Text("P/I No").Bold().FontSize(7);
                                    header.Cell().Text("Curr").Bold().FontSize(7);
                                    header.Cell().Text("Buyer").Bold().FontSize(7);
                                    header.Cell().Text("Order").Bold().FontSize(7);
                                    header.Cell().Text("Type").Bold().FontSize(7);
                                    header.Cell().Text("Style").Bold().FontSize(7);
                                    header.Cell().ColumnSpan(9).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                                });

                                foreach (var po in basisGroup.PurchaseOrders)
                                {
                                    foreach (var group in po.OutstandingGroups)
                                    {
                                        table.Cell().PaddingVertical(2).Text(po.PurchaseOrderNumber).FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(po.CreatedDate.HasValue ? po.CreatedDate.Value.ToString("dd-MMM-yyyy") : "-").FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(string.IsNullOrEmpty(po.SupplierName) ? po.SupplierCode : po.SupplierName).FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(po.ProformaInvoiceNo ?? "-").FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(po.CurrencyCode).FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(string.IsNullOrEmpty(group.BuyerName) ? group.BuyerCode.ToString() : group.BuyerName).FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(group.Order).FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(string.IsNullOrEmpty(group.TypeName) ? group.TypeCode.ToString() : group.TypeName).FontSize(7);
                                        table.Cell().PaddingVertical(2).Text(group.StyleCode).FontSize(7);
                                    }
                                }
                            });
                        }

                        if (report.BasisGroups.Count == 0)
                        {
                            column.Item().PaddingTop(10).Text("No outstanding Purchase Orders found for the given criteria.").Italic().FontColor(Colors.Grey.Darken1);
                        }
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
