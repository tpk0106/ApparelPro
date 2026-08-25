using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_OGTN2.PRG - print run for the General Inventory <->
    // Orderwise Inventory bridge Goods Transfer Note.
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
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("GOODS TRANSFER NOTE (General Inventory / Orders)").FontSize(13).Bold();
                            row.ConstantItem(160).Column(dateCol =>
                            {
                                dateCol.Item().AlignRight().Text($"Date : {details.Header.PrintedOn:dd/MM/yyyy}").FontSize(9);
                                dateCol.Item().AlignRight().Text($"Time : {details.Header.PrintedOn:HH:mm}").FontSize(9);
                            });
                        });
                        column.Item().PaddingTop(2).Text($"OGTN No : {details.Header.OgtnNumber}").Bold();

                        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Direction : ").Bold(); t.Span(directionLabel); });
                            row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span($"{details.Header.BuyerCode} - {details.Header.BuyerName}"); });
                            row.RelativeItem().Text(t => { t.Span("Order : ").Bold(); t.Span(details.Header.Order); });
                        });

                        column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
                    });

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
                                headerRow.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
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
                                sig.Item().PaddingTop(2).Text("Checked By").FontSize(9);
                            });
                            row.RelativeItem().Column(sig =>
                            {
                                sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                                sig.Item().PaddingTop(2).Text("Authorised By").FontSize(9);
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
