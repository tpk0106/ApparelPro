using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_GRN2.PRG (print run of a committed General
    // Inventory Goods Received Note). Same header-repeats / signature-once-at-end
    // structure as GeneralStrnPrintEngine/GeneralGinPrintEngine - see NotePrintEngineHelper.
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
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "GOODS RECEIVED NOTE (General Inventory)",
                        "GRN No",
                        details.Header.GrnNumber,
                        details.Header.PrintedOn,
                        detailsColumn =>
                        {
                            detailsColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text(t => { t.Span("P/O No : ").Bold(); t.Span(details.Header.PoNumber); });
                                row.RelativeItem().Text(t => { t.Span("Supplier : ").Bold(); t.Span(details.Header.SupplierCode); });
                                row.RelativeItem().Text(t => { t.Span("Currency : ").Bold(); t.Span(details.Header.CurrencyCode); });
                            });
                            detailsColumn.Item().PaddingTop(4).Row(row =>
                            {
                                row.RelativeItem().Text(t => { t.Span("Invoice No : ").Bold(); t.Span(details.Header.InvoiceNumber ?? ""); });
                            });
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
                                headerRow.Cell().ColumnSpan(6).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
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

                        column.RenderSignatureBlock();
                    });

                    page.RenderPageNumberFooter();
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
