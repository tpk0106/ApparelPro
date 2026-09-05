using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_GRN2.PRG (print run of an already-committed
    // Goods Received Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding shared with every note print engine.
    public class GrnPrintEngine
    {
        public static byte[] GenerateGrnPrintPdf(GrnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "GOODS RECEIVED NOTE",
                        "GRN No",
                        details.Header.GrnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span(details.Header.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order No : ").Bold(); t.Span(details.Header.Order); });
                            row.RelativeItem().Text(t => { t.Span("P/O No : ").Bold(); t.Span(details.Header.PoNumber); });
                            row.RelativeItem().Text(t => { t.Span("Supplier : ").Bold(); t.Span(details.Header.SupplierName); });
                            row.RelativeItem().Text(t => { t.Span("Basis : ").Bold(); t.Span(details.Header.StoreCode); });
                        }));

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(115);  // Item Code
                                columns.RelativeColumn(2.2f); // Description
                                columns.ConstantColumn(45);   // Unit
                                columns.ConstantColumn(75);   // Qty. Received
                                columns.ConstantColumn(70);   // Unit Price
                                columns.ConstantColumn(80);   // Value
                            });

                            table.Header(header =>
                            {
                                header.Cell().PaddingHorizontal(4).Text("Item Code").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Description").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Unit").Bold();
                                header.Cell().PaddingHorizontal(4).AlignRight().Text("Qty. Received").Bold();
                                header.Cell().PaddingHorizontal(4).AlignRight().Text("U/Price").Bold();
                                header.Cell().PaddingHorizontal(4).AlignRight().Text($"Value ({details.Lines.FirstOrDefault()?.Currency})").Bold();

                                header.Cell().ColumnSpan(6).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.Description);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.Unit);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).AlignRight().Text(line.Quantity.ToString("N2"));
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).AlignRight().Text(line.UnitPrice.ToString("N4"));
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).AlignRight().Text(line.Value.ToString("N2"));
                            }
                        });

                        column.Item().PaddingTop(8).AlignRight().Text(t =>
                        {
                            t.Span("Total Value : ").Bold();
                            t.Span(details.Lines.Sum(l => l.Value).ToString("N2")).Bold();
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
