using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderwiseInventory
{
    // Modern equivalent of legacy IN_DLIST.PRG's printed report.
    public static class TransactionListReportEngine
    {
        public static byte[] GenerateTransactionListReportPdf(
            TransactionListReportHeaderServiceModel header,
            List<TransactionListReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("List of Transactions (Order-wise)").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text($"{header.TransactionTypeName} from {header.FromDate:dd/MM/yyyy} to {header.ToDate:dd/MM/yyyy}");
                        if (!string.IsNullOrWhiteSpace(header.ItemCodePrefix))
                            column.Item().Text($"Stock/Item Code : {header.ItemCodePrefix}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);   // Type
                            columns.RelativeColumn(1.5f); // Date
                            columns.RelativeColumn(1.5f); // Doc No
                            columns.RelativeColumn(1);   // Basis
                            columns.RelativeColumn(2);   // Item Code
                            columns.RelativeColumn(3);   // Description
                            columns.RelativeColumn(1.3f); // Qty
                            columns.RelativeColumn(0.8f); // Unit
                            columns.RelativeColumn(1.3f); // Price
                            columns.RelativeColumn(1.5f); // Value
                            columns.RelativeColumn(0.8f); // Curr
                            columns.RelativeColumn(2.5f); // Supplier
                            columns.RelativeColumn(2.5f); // Buyer
                            columns.RelativeColumn(1.5f); // Order
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Type", "Date", "Doc No", "Basis", "Item Code", "Description", "Qty", "Unit",
                                "Price", "Value", "Curr", "Supplier", "Buyer", "Order"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(2).Text(title).Bold().FontSize(6.5f);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(2).Text(line.TransactionTypeName);
                            table.Cell().Padding(2).Text(line.TransactionDate.ToString("dd/MM/yy"));
                            table.Cell().Padding(2).Text(line.DocumentNumber);
                            table.Cell().Padding(2).Text(line.StoreCode);
                            table.Cell().Padding(2).Text(line.ItemCode);
                            table.Cell().Padding(2).Text(line.Description);
                            table.Cell().Padding(2).AlignRight().Text($"{line.Quantity:N2}");
                            table.Cell().Padding(2).Text(line.Unit);
                            table.Cell().Padding(2).AlignRight().Text($"{line.Price:N4}");
                            table.Cell().Padding(2).AlignRight().Text($"{line.Value:N2}");
                            table.Cell().Padding(2).Text(line.Currency);
                            table.Cell().Padding(2).Text(line.SupplierName);
                            table.Cell().Padding(2).Text(line.BuyerName);
                            table.Cell().Padding(2).Text(line.Order);
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
                                t.Span($"{header.TotalValue:N4} {header.TotalValueCurrency}").Bold();
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
