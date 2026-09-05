using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_STRN2.PRG (print run of an already-committed
    // Stores Requisition Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding shared with every General Inventory note
    // print engine too.
    public class StrnPrintEngine
    {
        public static byte[] GenerateStrnPrintPdf(StrnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "STORES REQUISITION NOTE",
                        "SRN No",
                        details.Header.StrnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span(details.Header.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order No : ").Bold(); t.Span(details.Header.Order); });
                            row.RelativeItem().Text(t => { t.Span("To Department : ").Bold(); t.Span(details.Header.DepartmentCode); });
                        }));

                    // Item table + (once, at the true end of the document) the
                    // signature block — see NotePrintEngineHelper.RenderSignatureBlock.
                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                // Widened from 70 to fit the full 22-char composite code
                                // (StockCode 2 + ItemCode 4 + Feature1-4 x4) now that the
                                // line query no longer decomposes it down to 4 characters.
                                columns.ConstantColumn(130);  // Item Code
                                columns.RelativeColumn(2.4f); // Description
                                columns.ConstantColumn(55);   // Unit
                                columns.ConstantColumn(95);   // Qty. Issued
                                columns.ConstantColumn(80);   // Basis
                            });

                            table.Header(header =>
                            {
                                header.Cell().PaddingHorizontal(4).Text("Item Code").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Description").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Unit").Bold();
                                header.Cell().PaddingHorizontal(4).AlignRight().Text("Qty. Issued").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Basis").Bold();

                                header.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.Description);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.Unit);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).AlignRight().Text(line.Quantity.ToString("N2"));
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4).Text(line.StoreCode);
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
