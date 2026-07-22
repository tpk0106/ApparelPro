using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using System;
using System.Collections.Generic;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_STRN2.PRG (print run of an already-committed
    // Stores Requisition Note). Mirrors the shared INV_HEAD/INV_FOOT routines from
    // OD_FUNCS.PRG: the document header (SRN No / date / time / Buyer / Order /
    // Department / column titles) repeats on every page via page.Header(), while the
    // Prepared By / Checked By / Authorised By signature block is only ever printed
    // once, at the very end of the whole document — legacy calls "do inv_foot" a
    // single time after the entire print loop finishes, not once per page. QuestPDF's
    // page.Content() is a single flowing column across all pages, so placing the
    // signature block after the table there reproduces that "once, at the end"
    // behaviour for free. page.Footer() is reserved for page numbering only.
    public class StrnPrintEngine
    {
        public static byte[] GenerateStrnPrintPdf(StrnPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using (var memoryStream = new MemoryStream())
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                        // Repeats on every page — legacy inv_head + the STRN-specific
                        // Buyer/Order/Department line and column header row.
                        page.Header().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("STORES REQUISITION NOTE").FontSize(14).Bold();
                                row.ConstantItem(160).Column(dateCol =>
                                {
                                    dateCol.Item().AlignRight().Text($"Date : {details.Header.PrintedOn:dd/MM/yyyy}").FontSize(9);
                                    dateCol.Item().AlignRight().Text($"Time : {details.Header.PrintedOn:HH:mm}").FontSize(9);
                                });
                            });
                            column.Item().PaddingTop(2).Text($"SRN No : {details.Header.StrnNumber}").Bold();

                            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);

                            column.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text(t => { t.Span("Buyer : ").Bold(); t.Span(details.Header.BuyerCode.ToString()); });
                                row.RelativeItem().Text(t => { t.Span("Order No : ").Bold(); t.Span(details.Header.Order); });
                                row.RelativeItem().Text(t => { t.Span("To Department : ").Bold(); t.Span(details.Header.DepartmentCode); });
                            });

                            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
                        });

                        // Item table + (once, at the true end of the document) the
                        // signature block — see class remarks above.
                        page.Content().PaddingTop(10).Column(column =>
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(70);   // Item Code
                                    columns.RelativeColumn(3);    // Description
                                    columns.ConstantColumn(50);   // Unit
                                    columns.ConstantColumn(80);   // Qty. Issued
                                    columns.ConstantColumn(70);   // Store
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Item Code").Bold();
                                    header.Cell().Text("Description").Bold();
                                    header.Cell().Text("Unit").Bold();
                                    header.Cell().AlignRight().Text("Qty. Issued").Bold();
                                    header.Cell().Text("Store").Bold();

                                    header.Cell().ColumnSpan(5).PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
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

                            // Prepared By / Checked By / Authorised By — printed once,
                            // after the last line item, regardless of how many pages
                            // the table above spanned.
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

                        // Page numbering only — the signature block above is NOT
                        // duplicated here, unlike the Stylewise Events report engine.
                        // Note: the Action<TextDescriptor> overload of .Text() returns
                        // void, so font size has to be set via DefaultTextStyle() inside
                        // the delegate rather than chained after the .Text(...) call.
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
}
