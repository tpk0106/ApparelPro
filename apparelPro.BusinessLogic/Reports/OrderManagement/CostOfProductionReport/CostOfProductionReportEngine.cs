using apparelPro.BusinessLogic.Services.Models.OrderManagement.ICostOfProductionReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.CostOfProductionReport
{
    // Replicates OD_FCOST.PRG's "COST OF PRODUCTION" printed layout - materials,
    // additional costs, sub contracts, style revenue, line costing, then the final
    // Estimated vs Actual profit margin.
    public static class CostOfProductionReportEngine
    {
        public static byte[] GeneratePdf(CostOfProductionReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ COST OF PRODUCTION ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span("Buyer: ").Bold(); t.Span($"{report.BuyerName} ({report.BuyerCode})    ");
                            t.Span("Order: ").Bold(); t.Span($"{report.Order}    ");
                            t.Span("Total Order Qty: ").Bold(); t.Span($"{report.TotalOrderQuantity:N2} {report.Unit}    ");
                            t.Span("Currency: ").Bold(); t.Span(report.CurrencyCode);
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(14);

                        // Materials
                        column.Item().Column(section =>
                        {
                            section.Item().Text("Materials Received").Bold().FontSize(10);
                            section.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(1.2f); c.RelativeColumn(2.5f); c.RelativeColumn(1f);
                                    c.RelativeColumn(1f); c.RelativeColumn(1f); c.RelativeColumn(1.2f);
                                });
                                table.Header(h =>
                                {
                                    foreach (var title in new[] { "Item Code", "Description", "Unit", "Quantity", "Price", "Value" })
                                        h.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                });
                                foreach (var m in report.Materials)
                                {
                                    table.Cell().Padding(2).Text(m.ItemCode);
                                    table.Cell().Padding(2).Text(m.Description);
                                    table.Cell().Padding(2).Text(m.Unit);
                                    table.Cell().Padding(2).AlignRight().Text($"{m.Quantity:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{m.Price:N4}");
                                    table.Cell().Padding(2).AlignRight().Text($"{m.Value:N2}");
                                }
                            });
                            section.Item().AlignRight().Text(t =>
                            {
                                t.Span("Total Materials Value: ").Bold();
                                t.Span($"{report.TotalMaterialsValue:N2}    (Estimated: {report.EstimatedMaterialsValue:N2})");
                            });
                        });

                        // Additional costs
                        if (report.AdditionalCostGroups.Count > 0)
                        {
                            column.Item().Column(section =>
                            {
                                section.Item().Text("Additional Costs").Bold().FontSize(10);
                                foreach (var group in report.AdditionalCostGroups)
                                {
                                    section.Item().Text(group.AdditionalCostDescription).SemiBold().FontSize(9);
                                    section.Item().PaddingLeft(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(c =>
                                        {
                                            c.RelativeColumn(1f); c.RelativeColumn(2f); c.RelativeColumn(1f);
                                            c.RelativeColumn(1f); c.RelativeColumn(1f); c.RelativeColumn(1f);
                                        });
                                        table.Header(h =>
                                        {
                                            foreach (var title in new[] { "Item Code", "Description", "Qty/Garment", "Received", "Price", "Cost" })
                                                h.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                        });
                                        foreach (var line in group.Lines)
                                        {
                                            table.Cell().Padding(2).Text(line.ItemCode);
                                            table.Cell().Padding(2).Text(line.Description);
                                            table.Cell().Padding(2).AlignRight().Text($"{line.QuantityPerGarment:N2}");
                                            table.Cell().Padding(2).AlignRight().Text($"{line.ReceivedQuantity:N2}");
                                            table.Cell().Padding(2).AlignRight().Text($"{line.PricePerUnit:N4}");
                                            table.Cell().Padding(2).AlignRight().Text($"{line.Cost:N2}");
                                        }
                                    });
                                }
                                section.Item().AlignRight().Text(t =>
                                {
                                    t.Span("Total Additional Cost: ").Bold();
                                    t.Span($"{report.TotalAdditionalCostValue:N2}    (Estimated: {report.EstimatedAdditionalCostValue:N2})");
                                });
                            });
                        }

                        // Sub contracts
                        if (report.SubContracts.Count > 0)
                        {
                            column.Item().Column(section =>
                            {
                                section.Item().Text("Sub Contracts").Bold().FontSize(10);
                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn(2f); c.RelativeColumn(1f); c.RelativeColumn(1f);
                                        c.RelativeColumn(1f); c.RelativeColumn(1.2f);
                                    });
                                    table.Header(h =>
                                    {
                                        foreach (var title in new[] { "Sub Contractor", "Cost/Garment", "Quantity", "Unit", "Cost" })
                                            h.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                    });
                                    foreach (var sc in report.SubContracts)
                                    {
                                        table.Cell().Padding(2).Text(sc.SubContractorName);
                                        table.Cell().Padding(2).AlignRight().Text($"{sc.CostPerGarment:N4}");
                                        table.Cell().Padding(2).AlignRight().Text($"{sc.Quantity:N2}");
                                        table.Cell().Padding(2).Text(sc.Unit);
                                        table.Cell().Padding(2).AlignRight().Text($"{sc.Cost:N2}");
                                    }
                                });
                                section.Item().AlignRight().Text(t =>
                                {
                                    t.Span("Total Sub Contract Cost: ").Bold();
                                    t.Span($"{report.TotalSubContractValue:N2}");
                                });
                            });
                        }

                        // Style revenue: estimated vs actual
                        column.Item().Column(section =>
                        {
                            section.Item().Text("Style Revenue - Estimated vs Actual").Bold().FontSize(10);
                            section.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(1.5f); c.RelativeColumn(1f);
                                    c.RelativeColumn(1f); c.RelativeColumn(1.2f);
                                    c.RelativeColumn(1f); c.RelativeColumn(1.2f);
                                });
                                table.Header(h =>
                                {
                                    foreach (var title in new[] { "Style", "Unit Price", "Est. Qty", "Est. Value", "Actual Qty", "Actual Value" })
                                        h.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                });
                                foreach (var s in report.StyleRevenues)
                                {
                                    table.Cell().Padding(2).Text($"{s.StyleCode}");
                                    table.Cell().Padding(2).AlignRight().Text($"{s.UnitPrice:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{s.EstimatedQuantity:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{s.EstimatedValue:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{s.ActualProducedQuantity + s.SubContractReceivedQuantity:N2}");
                                    table.Cell().Padding(2).AlignRight().Text($"{s.ActualProducedValue + s.SubContractReceivedValue:N2}");
                                }
                            });
                        });

                        // Line costing: estimated vs actual
                        if (report.LineCosts.Count > 0)
                        {
                            column.Item().Column(section =>
                            {
                                section.Item().Text("Production Line Cost - Estimated vs Actual").Bold().FontSize(10);
                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn(1f); c.RelativeColumn(1.5f);
                                        c.RelativeColumn(1f); c.RelativeColumn(1.2f);
                                        c.RelativeColumn(1f); c.RelativeColumn(1.2f);
                                    });
                                    table.Header(h =>
                                    {
                                        foreach (var title in new[] { "Style", "Line", "Est. Days", "Est. Cost", "Actual Days", "Actual Cost" })
                                            h.Cell().BorderBottom(1).Padding(2).Text(title).FontSize(7).Bold();
                                    });
                                    foreach (var l in report.LineCosts)
                                    {
                                        table.Cell().Padding(2).Text($"{l.StyleCode}");
                                        table.Cell().Padding(2).Text(l.LineDescription);
                                        table.Cell().Padding(2).AlignRight().Text($"{l.EstimatedDays:N1}");
                                        table.Cell().Padding(2).AlignRight().Text($"{l.EstimatedCost:N2}");
                                        table.Cell().Padding(2).AlignRight().Text($"{l.ActualDays:N1}");
                                        table.Cell().Padding(2).AlignRight().Text($"{l.ActualCost:N2}");
                                    }
                                });
                            });
                        }

                        // Final margin
                        column.Item().Column(section =>
                        {
                            section.Item().LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                            section.Item().PaddingTop(4).Row(row =>
                            {
                                row.RelativeItem().Text($"Profit Margin in {report.CurrencyCode}").Bold().FontSize(11);
                                row.ConstantItem(160).AlignRight().Text($"Estimated: {report.EstimatedProfitMargin:N2}").Bold();
                                row.ConstantItem(160).AlignRight().Text($"Actual: {report.ActualProfitMargin:N2}").Bold();
                            });
                        });
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
