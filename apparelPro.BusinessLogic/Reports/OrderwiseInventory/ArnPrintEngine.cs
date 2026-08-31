using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_ARN2.PRG (print run of an already-committed
    // Additional Goods Receipt Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding shared with every note print engine.
    public class ArnPrintEngine
    {
        public static byte[] GenerateArnPrintPdf(ArnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "ADDITIONAL GOODS RECEIPTS NOTE",
                        "ARN No",
                        details.Header.ArnNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Basis : ").Bold(); t.Span(details.Header.StoreCode); });
                            row.RelativeItem().Text(t => { t.Span("Invoice No : ").Bold(); t.Span(details.Header.InvoiceNumber ?? ""); });
                            row.RelativeItem().Text(t => { t.Span("Sub Cont : ").Bold(); t.Span(details.Header.SubContractorCode); });
                            row.RelativeItem().Text(t => { t.Span("Currency : ").Bold(); t.Span(details.Header.Currency); });
                        }));

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);  // Item Code
                                columns.RelativeColumn(2.2f); // Description
                                columns.ConstantColumn(40);   // Unit
                                columns.ConstantColumn(70);   // Qty Received
                                columns.ConstantColumn(65);   // Unit Price
                                columns.ConstantColumn(70);   // Value
                                columns.ConstantColumn(70);   // Bal. to Receive
                                columns.ConstantColumn(45);   // Buyer
                                columns.ConstantColumn(55);   // Order
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item Code").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Unit").Bold();
                                header.Cell().AlignRight().Text("Qty Rcvd").Bold();
                                header.Cell().AlignRight().Text("U/Price").Bold();
                                header.Cell().AlignRight().Text($"Value ({details.Header.Currency})").Bold();
                                header.Cell().AlignRight().Text("Bal. to Rcv").Bold();
                                header.Cell().Text("Buyer").Bold();
                                header.Cell().Text("Order").Bold();

                                header.Cell().ColumnSpan(9).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).Text(line.Description);
                                table.Cell().PaddingVertical(3).Text(line.Unit);
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Quantity.ToString("N2"));
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.UnitPrice.ToString("N4"));
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Value.ToString("N2"));
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.BalanceToReceive.ToString("N2"));
                                table.Cell().PaddingVertical(3).Text(line.BuyerCode.ToString());
                                table.Cell().PaddingVertical(3).Text(line.Order);
                            }
                        });

                        column.Item().PaddingTop(8).AlignRight().Text(t =>
                        {
                            t.Span($"Total Value ({details.Header.Currency}) : ").Bold();
                            t.Span(details.Header.TotalValue.ToString("N2")).Bold();
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
