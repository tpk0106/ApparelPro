using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_SRN2.PRG (print run of a committed General
    // Inventory Supplier Return Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding.
    public class GeneralSrtnPrintEngine
    {
        public static byte[] GenerateSrtnPrintPdf(GeneralSrtnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "SUPPLIER RETURN NOTE (General Inventory)",
                        "SRN No",
                        details.Header.SrtnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("From Stores : ").Bold(); t.Span(details.Header.StoreDescription); });
                            row.RelativeItem().Text(t => { t.Span("To Supplier : ").Bold(); t.Span(details.Header.SupplierName); });
                            row.RelativeItem().Text(t => { t.Span("Status : ").Bold(); t.Span(details.Header.StockType); });
                        }));

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(130);  // Item Code
                                columns.RelativeColumn(2.4f); // Description
                                columns.ConstantColumn(50);   // Unit
                                columns.ConstantColumn(80);   // Qty
                            });

                            table.Header(headerRow =>
                            {
                                headerRow.Cell().Text("Item Code").Bold();
                                headerRow.Cell().Text("Description").Bold();
                                headerRow.Cell().Text("Unit").Bold();
                                headerRow.Cell().AlignRight().Text("Qty. Returned").Bold();
                                headerRow.Cell().ColumnSpan(4).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
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
