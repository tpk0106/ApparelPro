using apparelPro.BusinessLogic.Reports.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_AIN2.PRG (print run of an already-committed
    // Additional Issue Note). See NotePrintEngineHelper for the shared
    // header/signature/footer scaffolding shared with every note print engine.
    public class AinPrintEngine
    {
        public static byte[] GenerateAinPrintPdf(AinPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ConfigureStandardPage();

                    page.RenderNoteHeader(
                        "ADDITIONAL ISSUE NOTE",
                        "AIN No",
                        details.Header.AinNumber,
                        details.Header.PrintedOn,
                        detailsColumn => detailsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span(details.Header.BuyerName); });
                            row.RelativeItem().Text(t => { t.Span("Order No : ").Bold(); t.Span(details.Header.Order); });
                            row.RelativeItem().Text(t => { t.Span("Sub Cont : ").Bold(); t.Span(details.Header.SubContractorCode); });
                            row.RelativeItem().Text(t => { t.Span("Process : ").Bold(); t.Span(details.Header.AdditionalProcessCode); });
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
                                columns.ConstantColumn(90);   // Qty. Issued
                                columns.ConstantColumn(70);   // Basis
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item Code").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Unit").Bold();
                                header.Cell().AlignRight().Text("Qty. Issued").Bold();
                                header.Cell().Text("Basis").Bold();

                                header.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
                                table.Cell().PaddingVertical(3).Text(line.ItemCode);
                                table.Cell().PaddingVertical(3).Text(line.Description);
                                table.Cell().PaddingVertical(3).Text(line.Unit);
                                table.Cell().PaddingVertical(3).AlignRight().Text(line.Quantity.ToString("N2"));
                                table.Cell().PaddingVertical(3).Text(line.StoreCode);
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
