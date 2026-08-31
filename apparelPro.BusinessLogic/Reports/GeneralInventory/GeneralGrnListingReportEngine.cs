using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_GRN4.PRG / GI_GRN5.PRG's printed report.
    public static class GeneralGrnListingReportEngine
    {
        public static byte[] GenerateGrnListingReportPdf(
            GeneralGrnListingReportHeaderServiceModel header,
            List<GeneralGrnListingReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            string title = header.StoreCode != null || header.SupplierCode != null
                ? "Goods Received Note Listing - Supplier Wise"
                : "Goods Received Note Listing - Date Wise";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(title).FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"From {header.FromDate:dd/MM/yyyy} to {header.ToDate:dd/MM/yyyy}    Transactions : {header.TotalTransactions}");
                        if (header.StoreCode != null)
                            column.Item().Text($"Store : {header.StoreCode} - {header.StoreDescription}");
                        if (header.SupplierCode != null)
                            column.Item().Text($"Supplier : {header.SupplierCode} - {header.SupplierName}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Date
                            columns.RelativeColumn(2);  // GRN No
                            columns.RelativeColumn(2);  // Invoice No
                            columns.RelativeColumn(2);  // PO No
                            columns.RelativeColumn(4);  // Supplier
                            columns.RelativeColumn(1);  // Store
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(4);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(2);  // Quantity
                            columns.RelativeColumn(2);  // Unit Price
                            columns.RelativeColumn(2);  // Amount
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var col in new[]
                            {
                                "Date", "GRN No", "Invoice No", "D/O No", "Supplier", "Store", "Item Code",
                                "Description", "Unit", "Quantity", "Unit Price", "Amount"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(col).Bold().FontSize(7.5f);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.TransactionDate.ToString("dd/MM/yy"));
                            table.Cell().Padding(3).Text(line.GrnNumber);
                            table.Cell().Padding(3).Text(line.InvoiceNumber ?? "");
                            table.Cell().Padding(3).Text(line.PoNumber ?? "");
                            table.Cell().Padding(3).Text(line.SupplierName);
                            table.Cell().Padding(3).Text(line.StoreCode);
                            table.Cell().Padding(3).Text(line.ItemCode);
                            table.Cell().Padding(3).Text(line.Description);
                            table.Cell().Padding(3).Text(line.Unit);
                            table.Cell().Padding(3).AlignRight().Text($"{line.Quantity:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.UnitPrice:N4}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.Amount:N2}");
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
                        column.Item().AlignRight().PaddingTop(4).Text($"Total : {header.TotalValue:N2}").Bold().FontSize(10);
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
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
