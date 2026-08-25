using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_GRN2.PRG (print run of a committed General
    // Inventory Goods Received Note). Same header-repeats / signature-once-at-end
    // structure as GeneralStrnPrintEngine/GeneralGinPrintEngine.
    public class GeneralGrnPrintEngine
    {
        public static byte[] GenerateGrnPrintPdf(GeneralGrnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("GOODS RECEIVED NOTE (General Inventory)").FontSize(14).Bold();
                            row.ConstantItem(160).Column(dateCol =>
                            {
                                dateCol.Item().AlignRight().Text($"Date : {details.Header.PrintedOn:dd/MM/yyyy}").FontSize(9);
                                dateCol.Item().AlignRight().Text($"Time : {details.Header.PrintedOn:HH:mm}").FontSize(9);
                            });
                        });
                        column.Item().PaddingTop(2).Text($"GRN No : {details.Header.GrnNumber}").Bold();

                        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("P/O No : ").Bold(); t.Span(details.Header.PoNumber); });
                            row.RelativeItem().Text(t => { t.Span("Supplier : ").Bold(); t.Span(details.Header.SupplierCode); });
                            row.RelativeItem().Text(t => { t.Span("Currency : ").Bold(); t.Span(details.Header.CurrencyCode); });
                        });
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Invoice No : ").Bold(); t.Span(details.Header.InvoiceNumber ?? ""); });
                        });

                        column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);   // Store
                                columns.ConstantColumn(120);  // Item Code
                                columns.RelativeColumn(2f);   // Description
                                columns.ConstantColumn(50);   // Unit
                                columns.ConstantColumn(80);   // Qty
                                columns.ConstantColumn(80);   // Price
                            });

                            table.Header(headerRow =>
                            {
                                headerRow.Cell().Text("Store").Bold();
                                headerRow.Cell().Text("Item Code").Bold();
                                headerRow.Cell().Text("Description").Bold();
                                headerRow.Cell().Text("Unit").Bold();
                                headerRow.Cell().AlignRight().Text("Quantity").Bold();
                                headerRow.Cell().AlignRight().Text("Price").Bold();
                                headerRow.Cell().ColumnSpan(6).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).Text(line.StoreCode);
                                table.Cell().PaddingVertical(3).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).Text(line.Description);
                                table.Cell().PaddingVertical(3).Text(line.Unit);
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Quantity.ToString("N2"));
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Price.ToString("N4"));
                            }
                        });

                        column.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(sig =>
                            {
                                sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                                sig.Item().PaddingTop(2).Text("Prepared By").FontSize(9);
                            });
                            row.RelativeItem().Column(sig =>
                            {
                                sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                                sig.Item().PaddingTop(2).Text("Checked By").FontSize(9);
                            });
                            row.RelativeItem().Column(sig =>
                            {
                                sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                                sig.Item().PaddingTop(2).Text("Authorised By").FontSize(9);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(TextStyle.Default.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
