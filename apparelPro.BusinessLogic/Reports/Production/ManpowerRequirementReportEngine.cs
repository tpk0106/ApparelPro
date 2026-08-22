using apparelPro.BusinessLogic.Services.Models.Production.IManpowerRequirementReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Production
{
    // Replicates PR_REP3.PRG's print output - "MANPOWER REQUIREMENT".
    public static class ManpowerRequirementReportEngine
    {
        public static byte[] GeneratePdf(ManpowerRequirementReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("[ MANPOWER REQUIREMENT ]").FontSize(13).Bold();
                        column.Item().PaddingTop(4).Text(
                            $"Buyer: {report.BuyerName}   Order: {report.Order}   Type: {report.TypeName}   Style: {report.StyleCode}" +
                            (report.LineCode != null ? $"   Line: {report.LineCode}" : ""));
                        column.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Black);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        void MachineTimeTable(string title, List<ManpowerMachineTimeRowServiceModel> rows)
                        {
                            column.Item().PaddingTop(6).Text(title).Bold().FontSize(10);
                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3.0f);
                                    columns.RelativeColumn(1.0f);
                                });
                                table.Header(headerRow =>
                                {
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).Text("Type of Machine").Bold();
                                    headerRow.Cell().BorderBottom(1.5f).Padding(2).AlignRight().Text("Time (SAM)").Bold();
                                });
                                foreach (var row in rows)
                                {
                                    table.Cell().Padding(2).Text(row.MachineTypeDescription);
                                    table.Cell().Padding(2).AlignRight().Text($"{row.TotalSam:N2}");
                                }
                            });
                        }

                        MachineTimeTable("Machine-Operated Types", report.MachineRows);
                        column.Item().PaddingTop(4).AlignRight().Text($"Total Machine Time : {report.TotalMachineTimeSam:N2}").Bold();

                        if (report.ManualRows.Count > 0)
                            MachineTimeTable("Manual Types", report.ManualRows);

                        column.Item().PaddingTop(8).Text($"Total (Machine + Manual)   : {report.GrandTotalSam:N2}").Bold();
                        column.Item().PaddingTop(4).Text($"Total Machine Time         : {report.TotalMachineTimeSam:N2}");
                        column.Item().Text($"PCS per Machine AT 100.0 % : {report.PcsPerMachineAt100:N2}");
                        column.Item().Text($"                AT {report.Eff1Percent}% : {report.PcsPerMachineAtEff1:N2}");
                        if (report.PcsPerMachineAtEff2.HasValue)
                            column.Item().Text($"                AT {report.Eff2Percent}% : {report.PcsPerMachineAtEff2:N2}");

                        column.Item().PaddingTop(8).Text("Target Output/Day (PCS/DAY)").Bold();
                        column.Item().PaddingTop(2).Text($"AT 100.0% Efficiency : {report.TargetOutputAt100:N0}");
                        column.Item().Text($"AT {report.Eff1Percent}% Efficiency : {report.TargetOutputAtEff1:N0}");
                        if (report.TargetOutputAtEff2.HasValue)
                            column.Item().Text($"AT {report.Eff2Percent}% Efficiency : {report.TargetOutputAtEff2:N0}");

                        column.Item().PaddingTop(8).Text($"ESTIMATED STANDARD HOURS -> {report.EstimatedStandardHours:N2}").Bold();
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
