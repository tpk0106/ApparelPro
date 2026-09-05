using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_GTN2.PRG (print run of an already-committed
    // Goods Transfer Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding shared with every note print engine.
    public class GtnPrintEngine
    {
        public static byte[] GenerateGtnPrintPdf(GtnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "GOODS TRANSFER NOTE",
                        "GTN No",
                        details.Header.GtnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("From Buyer : ").Bold(); t.Span(details.Header.FromBuyerName); });
                            row.RelativeItem().Text(t => { t.Span("From Order : ").Bold(); t.Span(details.Header.FromOrder); });
                            row.RelativeItem().Text(t => { t.Span("To Buyer : ").Bold(); t.Span(details.Header.ToBuyerName); });
                            row.RelativeItem().Text(t => { t.Span("To Order : ").Bold(); t.Span(details.Header.ToOrder); });
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
                                columns.ConstantColumn(95);   // Qty. Transferred
                                columns.ConstantColumn(80);   // Basis
                            });

                            table.Header(header =>
                            {
                                header.Cell().PaddingHorizontal(4).Text("Item Code").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Description").Bold();
                                header.Cell().PaddingHorizontal(4).Text("Unit").Bold();
                                header.Cell().PaddingHorizontal(4).AlignRight().Text("Qty. Transferred").Bold();
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
