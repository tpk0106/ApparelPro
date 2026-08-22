using apparelPro.BusinessLogic.Services.Models.Production.IOperationBreakdownReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_REP1.PRG's print output - "BREAKDOWN OPERATION: TARGET
    // QUANTITY PER OPERATION". One table per Component group, each with its
    // own header row.
    public static class OperationBreakdownReportEngine
    {
        public static byte[] GeneratePdf(OperationBreakdownReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ BREAKDOWN OPERATION: TARGET QUANTITY PER OPERATION ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text(
                            $"Buyer: {report.BuyerName}   Order: {report.Order}   Type: {report.TypeName}   Style: {report.StyleCode}");
                        column.Item().PaddingTop(2).Text(
                            $"Eff1: {report.Eff1Percent}%   Eff2: {report.Eff2Percent}%   Work Hours/Day: {report.WorkHoursPerDay}");
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        foreach (var group in report.Groups)
                        {
                            column.Item().PaddingTop(6).Text($"{group.ComponentCode} — {group.ComponentDescription}").Bold().FontSize(10);

                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.0f);  // Oper. No
                                    columns.RelativeColumn(1.2f);  // Oper. Code
                                    columns.RelativeColumn(2.6f);  // Description
                                    columns.RelativeColumn(1.2f);  // Machine
                                    columns.RelativeColumn(1.0f);  // SAM
                                    columns.RelativeColumn(1.2f);  // Quota @100%
                                    columns.RelativeColumn(1.2f);  // Quota @Eff1%
                                    columns.RelativeColumn(1.3f);  // Pcs/2Hrs @Eff1%
                                    columns.RelativeColumn(1.2f);  // Quota @Eff2%
                                    columns.RelativeColumn(1.3f);  // Pcs/2Hrs @Eff2%
                                    columns.RelativeColumn(1.1f);  // No. Mach.
                                    columns.RelativeColumn(1.2f);  // No. Operators
                                });

                                table.Header(headerRow =>
                                {
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Oper. No").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Oper. Code").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Description").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Machine").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text("SAM").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text("Quota @100%").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text($"Quota @{report.Eff1Percent}%").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text($"Pcs/2Hrs @{report.Eff1Percent}%").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text($"Quota @{report.Eff2Percent}%").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text($"Pcs/2Hrs @{report.Eff2Percent}%").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text("No. Mach.").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text("No. Operators").Bold();
                                });

                                foreach (var row in group.Rows)
                                {
                                    table.Cell().Padding(2).Text($"{row.DisplayOperationNo}");
                                    table.Cell().Padding(2).Text(row.OperationCode);
                                    table.Cell().Padding(2).Text(row.OperationDescription);
                                    table.Cell().Padding(2).Text(row.MachineTypeCode);
                                    table.Cell().Padding(2).AlignRight().Text($"{row.Sam:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.QuotaAt100:N0}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.QuotaAtEff1:N0}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.QuotaPcsPer2HrsAtEff1:N0}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.QuotaAtEff2:N0}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.QuotaPcsPer2HrsAtEff2:N0}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.NumberOfMachines:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{row.NumberOfOperators:N0}");
                                }
                            });
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
