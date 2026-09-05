using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_GRN3.PRG / IN_GRN4.PRG's printed report.
    public static class GrnListingReportEngine
    {
        public static byte[] GenerateGrnListingReportPdf(
            GrnListingReportHeaderServiceModel header,
            List<GrnListingReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            string title = header.BuyerCode.HasValue
                ? $"GRN Listing for {header.BuyerName}/{header.Order}"
                : "GRN Listing - Date Wise";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.3f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7.5f));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(title).FontSize(13).Bold();
                        if (header.FromDate.HasValue && header.ToDate.HasValue)
                            column.Item().PaddingTop(4).Text($"From {header.FromDate:dd/MM/yyyy} to {header.ToDate:dd/MM/yyyy}    Transactions : {header.TotalTransactions}");
                        else
                            column.Item().PaddingTop(4).Text($"Transactions : {header.TotalTransactions}");
                        if (header.StoreCode != null)
                            column.Item().Text($"Basis : {header.StoreCode}");
                        if (header.SupplierName != null)
                            column.Item().Text($"Supplier : {header.SupplierName}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f); // Date
                            columns.RelativeColumn(1.3f); // GRN No
                            columns.RelativeColumn(1.5f); // Invoice No
                            columns.RelativeColumn(1.3f); // PO No
                            columns.RelativeColumn(1.3f); // LC No
                            columns.RelativeColumn(1);   // Basis
                            columns.RelativeColumn(2);   // Item Code
                            columns.RelativeColumn(3);   // Description
                            columns.RelativeColumn(0.8f); // Unit
                            columns.RelativeColumn(1.3f); // Quantity
                            columns.RelativeColumn(1.3f); // Unit Price
                            columns.RelativeColumn(1.5f); // Value
                            columns.RelativeColumn(0.8f); // Curr
                            columns.RelativeColumn(2.5f); // Supplier
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var col in new[]
                            {
                                "Date", "GRN No", "Invoice No", "PO No", "LC No", "Basis", "Item Code",
                                "Description", "Unit", "Quantity", "Unit Price", "Value", "Curr", "Supplier"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).Text(col).Bold().FontSize(6.5f);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(2).Text(line.TransactionDate.ToString("dd/MM/yy"));
                            table.Cell().Padding(2).Text(line.GrnNumber);
                            table.Cell().Padding(2).Text(line.InvoiceNumber ?? "");
                            table.Cell().Padding(2).Text(line.PoNumber ?? "");
                            table.Cell().Padding(2).Text(line.LcNumber ?? "");
                            table.Cell().Padding(2).Text(line.StoreCode);
                            table.Cell().Padding(2).Text(line.ItemCode);
                            table.Cell().Padding(2).Text(line.Description);
                            table.Cell().Padding(2).Text(line.Unit);
                            table.Cell().Padding(2).AlignRight().Text($"{line.Quantity:N2}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.UnitPrice:N4}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.Value:N2}");
                            table.Cell().Padding(2).Text(line.Currency);
                            table.Cell().Padding(2).Text(line.SupplierName);
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        if (header.TotalValueCurrency != null)
                        {
                            column.Item().AlignRight().PaddingTop(4).Text(t =>
                            {
                                t.Span("Total : ").Bold();
                                t.Span($"{header.TotalValue:N2} {header.TotalValueCurrency}").Bold();
                            });
                        }
                        column.Item().AlignCenter().PaddingTop(4).Text("[ End of Report ]").Bold();
                        column.Item().AlignRight().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
