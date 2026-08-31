using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_FPO.PRG - "PURCHASE ORDER (GENERAL)" printable
    // form. See NotePrintEngineHelper for the shared header/signature/footer
    // scaffolding; the Order Date (a business date, distinct from the Date/Time-printed
    // pair every note header shows) moves into the detail row alongside Currency/P.I.
    // No/P.I. Date, same place every other note's own business fields live.
    public class GeneralPoPrintEngine
    {
        public static byte[] GeneratePoPrintPdf(GeneralPoPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "PURCHASE ORDER (General Inventory)",
                        "P/O No",
                        details.Header.PoNumber,
                        details.Header.PrintedOn,
                        detailsColumn =>
                        {
                            detailsColumn.Item().Text(t => { t.Span("To : ").Bold(); t.Span($"{details.Header.SupplierCode} - {details.Header.SupplierName}"); });
                            detailsColumn.Item().PaddingTop(4).Row(row =>
                            {
                                row.RelativeItem().Text(t =>
                                {
                                    t.Span("Order Date : ").Bold();
                                    t.Span(details.Header.OrderDate.HasValue ? details.Header.OrderDate.Value.ToString("dd/MM/yyyy") : "");
                                });
                                row.RelativeItem().Text(t => { t.Span("Currency : ").Bold(); t.Span(details.Header.CurrencyCode); });
                                row.RelativeItem().Text(t => { t.Span("P/I No : ").Bold(); t.Span(details.Header.ProformaInvoiceNo ?? ""); });
                                row.RelativeItem().Text(t =>
                                {
                                    t.Span("P/I Date : ").Bold();
                                    t.Span(details.Header.ProformaInvoiceDate.HasValue ? details.Header.ProformaInvoiceDate.Value.ToString("dd/MM/yyyy") : "");
                                });
                            });
                        });

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);   // Ref No
                                columns.ConstantColumn(90);   // Item Code
                                columns.RelativeColumn(2.0f); // Description
                                columns.ConstantColumn(45);   // Unit
                                columns.ConstantColumn(70);   // Qty
                                columns.ConstantColumn(65);   // Price
                                columns.ConstantColumn(75);   // Delivery Date
                            });

                            table.Header(headerRow =>
                            {
                                headerRow.Cell().Text("Ref No").Bold();
                                headerRow.Cell().Text("Item Code").Bold();
                                headerRow.Cell().Text("Description").Bold();
                                headerRow.Cell().Text("Unit").Bold();
                                headerRow.Cell().AlignRight().Text("Quantity").Bold();
                                headerRow.Cell().AlignRight().Text("U/Price").Bold();
                                headerRow.Cell().Text("Delivery").Bold();
                                headerRow.Cell().ColumnSpan(7).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).Text(line.RefNo);
                                table.Cell().PaddingVertical(3).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).Text(line.Description);
                                table.Cell().PaddingVertical(3).Text(line.Unit);
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.OrderedQuantity.ToString("N2"));
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Price.ToString("N4"));
                                table.Cell().PaddingVertical(3).Text(line.ExpectedDate.HasValue ? line.ExpectedDate.Value.ToString("dd/MM/yyyy") : "");
                            }
                        });

                        column.Item().PaddingTop(10).Text("Please, state our P/O No. on your delivery note.").Italic();

                        column.RenderSignatureBlock("Prepared By", "Purchasing Officer", "Director/G.Manager");
                    });

                    page.RenderPageNumberFooter();
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
