using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_DGN2.PRG (print run of an already-committed
    // Damaged Goods Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding shared with every note print engine.
    public class DgnPrintEngine
    {
        public static byte[] GenerateDgnPrintPdf(DgnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "DAMAGED GOODS NOTE",
                        "DGN No",
                        details.Header.DgnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span(details.Header.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order No : ").Bold(); t.Span(details.Header.Order); });
                        }));

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(130);  // Item Code
                                columns.RelativeColumn(2.4f); // Description
                                columns.ConstantColumn(55);   // Unit
                                columns.ConstantColumn(95);   // Qty. Damaged
                                columns.ConstantColumn(80);   // Basis
                            });

                            table.Header(header =>
                            {
                                header.Cell().PaddingHorizontal(4).Text("Item Code").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Description").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Unit").Bold();
                                header.Cell().PaddingHorizontal(4).AlignRight().Text("Qty. Damaged").Bold();
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
