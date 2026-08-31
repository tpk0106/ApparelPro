using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_OGTN2.PRG - print run for the General Inventory <->
    // Orderwise Inventory bridge Goods Transfer Note. See NotePrintEngineHelper for the
    // shared header/signature/footer scaffolding.
    public class OrderGtnPrintEngine
    {
        public static byte[] GenerateOgtnPrintPdf(OrderGtnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            string directionLabel = details.Header.Direction == OrderGtnDirection.GeneralToOrder
                ? "General Stores -> Buyer/Order"
                : "Buyer/Order -> General Stores";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "GOODS TRANSFER NOTE (General Inventory / Orders)",
                        "OGTN No",
                        details.Header.OgtnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Direction : ").Bold(); t.Span(directionLabel); });
                            row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span($"{details.Header.BuyerCode} - {details.Header.BuyerName}"); });
                            row.RelativeItem().Text(t => { t.Span("Order : ").Bold(); t.Span(details.Header.Order); });
                        }));

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);   // Store
                                columns.ConstantColumn(120);  // Item Code
                                columns.RelativeColumn(2.2f); // Description
                                columns.ConstantColumn(50);   // Unit
                                columns.ConstantColumn(80);   // Qty
                            });

                            table.Header(headerRow =>
                            {
                                headerRow.Cell().Text("Store").Bold();
                                headerRow.Cell().Text("Item Code").Bold();
                                headerRow.Cell().Text("Description").Bold();
                                headerRow.Cell().Text("Unit").Bold();
                                headerRow.Cell().AlignRight().Text("Quantity").Bold();
                                headerRow.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).Text(line.StoreCode);
                                table.Cell().PaddingVertical(3).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).Text(line.Description);
                                table.Cell().PaddingVertical(3).Text(line.Unit);
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Quantity.ToString("N2"));
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
