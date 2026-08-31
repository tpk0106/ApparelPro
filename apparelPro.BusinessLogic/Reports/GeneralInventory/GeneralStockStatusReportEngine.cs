using System.Globalization;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.GeneralInventory
{
    // Modern equivalent of legacy GI_SSTAT.PRG's printed report.
    public static class GeneralStockStatusReportEngine
    {
        public static byte[] GenerateStockStatusReportPdf(
            GeneralStockStatusReportHeaderServiceModel header,
            List<GeneralStockStatusReportLineServiceModel> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            string monthLabel = new DateTime(header.Year, header.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text($"Stock Status Report for {monthLabel}").FontSize(14).Bold();
                        column.Item().PaddingTop(5).Text($"Stores : {header.StoreCode} - {header.StoreDescription}");
                        column.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Item Code
                            columns.RelativeColumn(3);  // Description
                            columns.RelativeColumn(1);  // Unit
                            columns.RelativeColumn(2);  // B/F Balance
                            columns.RelativeColumn(2);  // Tot. GRNs
                            columns.RelativeColumn(2);  // Tot. GINs
                            columns.RelativeColumn(2);  // Tot. GTNs(in)
                            columns.RelativeColumn(2);  // Tot. GTNs(out)
                            columns.RelativeColumn(2);  // Tot. RTNs
                            columns.RelativeColumn(2);  // Tot. SRNs
                            columns.RelativeColumn(2);  // Tot. DGNs
                            columns.RelativeColumn(2);  // Last SAN
                            columns.RelativeColumn(2);  // C/F Balance
                        });

                        table.Header(headerRow =>
                        {
                            foreach (var title in new[]
                            {
                                "Item Code", "Description", "Unit", "B/F Balance", "Tot. GRNs",
                                "Tot. GINs", "Tot. GTNs(in)", "Tot. GTNs(out)", "Tot. RTNs",
                                "Tot. SRNs", "Tot. DGNs", "Last SAN", "C/F Balance"
                            })
                            {
                                headerRow.Cell().BorderBottom(1.5f).Padding(3).Text(title).Bold().FontSize(8);
                            }
                        });

                        foreach (var line in lines)
                        {
                            table.Cell().Padding(3).Text(line.ItemCode);
                            table.Cell().Padding(3).Text(line.Description);
                            table.Cell().Padding(3).Text(line.Unit);
                            table.Cell().Padding(3).AlignRight().Text($"{line.BroughtForwardBalance:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalGrns:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalGins:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalGtnsIn:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalGtnsOut:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalRtns:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalSrns:N2}");
                            table.Cell().Padding(3).AlignRight().Text($"{line.TotalDgns:N2}");
                            table.Cell().Padding(3).AlignRight().Text(line.LastSan.HasValue ? $"{line.LastSan:N2}" : "");
                            table.Cell().Padding(3).AlignRight().Text($"{line.CarriedForwardBalance:N2}").Bold();
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Black);
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
