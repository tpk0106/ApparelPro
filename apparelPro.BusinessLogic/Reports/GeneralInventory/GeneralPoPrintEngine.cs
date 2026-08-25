using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_FPO.PRG - "PURCHASE ORDER (GENERAL)" printable form.
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
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("PURCHASE ORDER (General Inventory)").FontSize(14).Bold();
                            row.ConstantItem(160).Column(dateCol =>
                            {
                                dateCol.Item().AlignRight().Text($"P/O No : {details.Header.PoNumber}").FontSize(9).Bold();
                                dateCol.Item().AlignRight().Text($"Date : {(details.Header.OrderDate.HasValue ? details.Header.OrderDate.Value.ToString("dd/MM/yyyy") : "")}").FontSize(9);
                            });
                        });

                        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);

                        column.Item().PaddingTop(6).Text(t => { t.Span("To : ").Bold(); t.Span($"{details.Header.SupplierCode} - {details.Header.SupplierName}"); });
                        column.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Currency : ").Bold(); t.Span(details.Header.CurrencyCode); });
                            row.RelativeItem().Text(t => { t.Span("P/I No : ").Bold(); t.Span(details.Header.ProformaInvoiceNo ?? ""); });
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("P/I Date : ").Bold();
                                t.Span(details.Header.ProformaInvoiceDate.HasValue ? details.Header.ProformaInvoiceDate.Value.ToString("dd/MM/yyyy") : "");
                            });
                        });

                        column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
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
                                headerRow.Cell().ColumnSpan(7).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
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
                                sig.Item().PaddingTop(2).Text("Purchasing Officer").FontSize(9);
                            });
                            row.RelativeItem().Column(sig =>
                            {
                                sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                                sig.Item().PaddingTop(2).Text("Director/G.Manager").FontSize(9);
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
