using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPendingEventsReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.PendingEventsReport
{
    // Replicates OD_EVPND.PRG's "PENDING EVENTS" printed layout - one group per
    // Buyer/Order/Type/Style, each listing its still-pending milestone events.
    public static class PendingEventsReportEngine
    {
        public static byte[] GeneratePdf(PendingEventsReportServiceModel report)
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
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ PENDING EVENTS ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span("As Of Date: ").Bold();
                            t.Span($"{report.AsOfDate:dd/MM/yyyy}");
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(10);

                        foreach (var group in report.Groups)
                        {
                            column.Item().Column(groupCol =>
                            {
                                groupCol.Item().Background(Colors.Grey.Lighten4).Padding(4).Text(t =>
                                {
                                    t.Span("Buyer: ").SemiBold(); t.Span($"{group.BuyerName}    ");
                                    t.Span("Order: ").SemiBold(); t.Span($"{group.Order}    ");
                                    t.Span("Type: ").SemiBold(); t.Span($"{group.TypeName}    ");
                                    t.Span("Style: ").SemiBold(); t.Span(group.StyleCode);
                                });

                                groupCol.Item().PaddingLeft(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(50);   // Event Code
                                        columns.RelativeColumn(2.5f); // Description
                                        columns.ConstantColumn(70);   // Scheduled Date
                                        columns.RelativeColumn(3f);   // Remarks
                                        columns.ConstantColumn(60);   // Delay
                                    });

                                    table.Header(header =>
                                    {
                                        foreach (var title in new[] { "Event Code", "Description", "Scheduled Date", "Remarks", "Delay" })
                                            header.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                    });

                                    foreach (var evt in group.Events)
                                    {
                                        table.Cell().Padding(2).Text(evt.EventCode);
                                        table.Cell().Padding(2).Text(evt.Description);
                                        table.Cell().Padding(2).Text(evt.ScheduledDate.HasValue ? $"{evt.ScheduledDate:dd-MMM-yy}" : "***");
                                        table.Cell().Padding(2).Text(evt.Remarks ?? "");
                                        table.Cell().Padding(2).Text(evt.DelayDays.HasValue ? $"{evt.DelayDays} Days" : "No Scheduled Date");
                                    }
                                });
                            });
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
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
