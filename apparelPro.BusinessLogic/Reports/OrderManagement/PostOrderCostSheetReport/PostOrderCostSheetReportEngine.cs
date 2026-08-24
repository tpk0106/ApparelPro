using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPostOrderCostSheetReportService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.OrderManagement.PostOrderCostSheetReport
{
    // Replicates OD_PCOST.PRG's "POST ORDER COST SHEET" printed layout.
    public static class PostOrderCostSheetReportEngine
    {
        public static byte[] GeneratePdf(PostOrderCostSheetReportServiceModel report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("[ POST ORDER COST SHEET ]").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            row.ConstantItem(120).AlignRight().Text($"Date: {DateTime.Now:dd-MMM-yyyy}").FontSize(8).Italic();
                        });
                        column.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span("Buyer: ").Bold(); t.Span(report.BuyerName + "   ");
                            t.Span("Order: ").Bold(); t.Span(report.Order);
                        });
                        column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(6);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Quantity - ORDERED: ").Bold();
                                t.Span($"{report.TotalOrderQuantity:N0}");
                            });
                            row.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Date - Order Accepted: ").Bold();
                                t.Span($"{report.OrderDate:dd/MM/yyyy}");
                            });
                        });

                        foreach (var style in report.Styles)
                        {
                            column.Item().Text($"Type: {style.TypeName}   Style: {style.StyleCode}   Unit: {style.Unit}   Unit Price: {style.UnitPrice:N2}   Basis: {report.BasisCode}").FontSize(7.5f);
                        }

                        column.Item().LineHorizontal(0.75f);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1f);
                            });
                            foreach (var section in report.SectionQuantities)
                            {
                                table.Cell().Padding(2).Text(section.SectionDescription + (section.IsFinal ? " (Final)" : ""));
                                table.Cell().Padding(2).AlignRight().Text($"{section.Quantity:N0}");
                            }
                        });

                        column.Item().AlignRight().Text($"Total Value Of Sales: {report.TotalValueOfSales:N2}").Bold();

                        column.Item().LineHorizontal(0.75f);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1.3f);
                            });

                            table.Header(headerRow =>
                            {
                                headerRow.Cell().BorderBottom(1f).Padding(3).Text("Description").Bold();
                                headerRow.Cell().BorderBottom(1f).Padding(3).AlignRight().Text($"Per Piece ({report.CurrencyCode})").Bold();
                                headerRow.Cell().BorderBottom(1f).Padding(3).AlignRight().Text($"Per Dozen ({report.CurrencyCode})").Bold();
                                headerRow.Cell().BorderBottom(1f).Padding(3).AlignRight().Text("Total Value").Bold();
                            });

                            foreach (var m in report.MaterialGroups)
                            {
                                table.Cell().Padding(2).Text(m.StockCategoryDescription);
                                table.Cell().Padding(2).AlignRight().Text($"{m.PerPieceCost:N4}");
                                table.Cell().Padding(2).AlignRight().Text($"{m.PerDozenCost:N4}");
                                table.Cell().Padding(2).AlignRight().Text($"{m.TotalValue:N2}");
                            }

                            table.Cell().Padding(2).Text("T O T A L").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.MaterialsPerPieceCost:N4}").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.MaterialsPerDozenCost:N4}").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.MaterialsTotalValue:N2}").Bold();

                            table.Cell().Padding(2).Text("PRODUCTION COST (In House)");
                            table.Cell().Padding(2).AlignRight().Text($"{report.ProductionCostPerPiece:N4}");
                            table.Cell().Padding(2).AlignRight().Text($"{report.ProductionCostPerDozen:N4}");
                            table.Cell().Padding(2).AlignRight().Text($"{report.ProductionCostTotalValue:N2}");

                            foreach (var a in report.AdditionalCostGroups)
                            {
                                table.Cell().Padding(2).Text(a.AdditionalCostDescription);
                                table.Cell().Padding(2).AlignRight().Text($"{a.PerPieceCost:N4}");
                                table.Cell().Padding(2).AlignRight().Text($"{a.PerDozenCost:N4}");
                                table.Cell().Padding(2).AlignRight().Text($"{a.TotalValue:N2}");
                            }

                            if (report.SubContractTotalValue != 0 || report.SubContractPerPiece != 0)
                            {
                                table.Cell().Padding(2).Text("SUB CONTRACT");
                                table.Cell().Padding(2).AlignRight().Text($"{report.SubContractPerPiece:N4}");
                                table.Cell().Padding(2).AlignRight().Text($"{report.SubContractPerDozen:N4}");
                                table.Cell().Padding(2).AlignRight().Text($"{report.SubContractTotalValue:N2}");
                            }

                            table.Cell().Padding(2).Text("T O T A L").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.ProductionTotalPerPiece:N4}").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.ProductionTotalPerDozen:N4}").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.ProductionTotalValue:N2}").Bold();

                            table.Cell().Padding(2).Text("G R A N D  T O T A L").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.GrandTotalPerPiece:N4}").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.GrandTotalPerDozen:N4}").Bold();
                            table.Cell().Padding(2).AlignRight().Text($"{report.GrandTotalValue:N2}").Bold();
                        });

                        column.Item().LineHorizontal(0.75f);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"No of days Utilised for prod. - {report.DaysUtilised} Day{(report.DaysUtilised == 1 ? "" : "s")}");
                                c.Item().Text($"Average day production - {report.AverageDayProduction:N0} Pcs");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text($"G R O S S  P R O F I T: {report.GrossProfit:N2}").Bold();
                                c.Item().PaddingTop(6).AlignRight().Text($"LESS - Finance Charges ({report.PercentOfTotalValue:N2}%): {report.FinanceCharges:N2}");
                                c.Item().AlignRight().Text($"Freight Charges: {report.FreightCharges:N2}");
                                c.Item().PaddingTop(8).AlignRight().Text($"N E T  P R O F I T: {report.NetProfit:N2}").Bold();
                                c.Item().PaddingTop(6).AlignRight().Text($"Net Profit on sales: {report.NetProfitOnSalesPercent:N2}%").Bold();
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
