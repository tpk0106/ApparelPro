using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_STRN2.PRG (print run of a committed General
    // Inventory Stores Requisition Note). Same header-repeats / signature-once-at-end
    // structure as StrnPrintEngine (Orderwise's counterpart).
    public class GeneralStrnPrintEngine
    {
        public static byte[] GenerateStrnPrintPdf(GeneralStrnPrintDetailsServiceModel details)
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
                            row.RelativeItem().Text("STORES REQUISITION NOTE (General Inventory)").FontSize(14).Bold();
                            row.ConstantItem(160).Column(dateCol =>
                            {
                                dateCol.Item().AlignRight().Text($"Date : {details.Header.PrintedOn:dd/MM/yyyy}").FontSize(9);
                                dateCol.Item().AlignRight().Text($"Time : {details.Header.PrintedOn:HH:mm}").FontSize(9);
                            });
                        });
                        column.Item().PaddingTop(2).Text($"SRN No : {details.Header.SrnNumber}").Bold();

                        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("From Stores : ").Bold(); t.Span(details.Header.StoreDescription); });
                            row.RelativeItem().Text(t => { t.Span("To Department : ").Bold(); t.Span(details.Header.DepartmentCode); });
                        });

                        column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
                    });

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
                                headerRow.Cell().AlignRight().Text("Quantity").Bold();
                                headerRow.Cell().ColumnSpan(4).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                            });

                            foreach (var line in details.Lines)
                            {
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
