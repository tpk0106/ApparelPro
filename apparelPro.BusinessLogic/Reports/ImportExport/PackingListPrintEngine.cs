using apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Redraws the Packing List print, migrated from legacy IE_PACK3.PRG
    // (Hanging Garments) and IE_PACK4.PRG (Cartons) - same "digital
    // overlay" dark/copper theme as the other IE print engines. No real
    // pre-printed form was supplied for this one (queued, see
    // project_packing_list_module memory) - laid out from the legacy
    // @row,col positions and column groupings instead.
    //
    // Legacy's own "Print (Cartons)" format never actually printed the
    // carton/color/size breakdown table (only the header + free-text
    // memo) - confirmed by reading ie_pack4.prg in full. Per explicit
    // decision, this engine's Cartons format goes further than legacy and
    // does print the real table, matching Hanging Garments' own approach
    // - the header + Detail memo, then a structured table.
    public static class PackingListPrintEngine
    {
        private const string PageBg = "#0A0E14";
        private const string TextWhite = "#F4F6F8";
        private const string TextMuted = "#8B93A1";
        private const string Copper = "#C9803D";

        private static void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1.0f, Unit.Centimetre);
            page.PageColor(PageBg);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8f).FontColor(TextWhite));
        }

        private static void PageNumberFooter(PageDescriptor page)
        {
            page.Footer().AlignCenter().Text(x =>
            {
                x.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(TextMuted));
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        }

        private static void HeaderBlock(ColumnDescriptor column, PackingListPrintDetailsServiceModel details, string title)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(title).Bold().FontSize(13).FontColor(Copper);
                row.RelativeItem().AlignRight().Text($"{details.InvoiceNumber} of {details.InvoiceDate}").FontSize(9);
            });
            column.Item().PaddingBottom(6);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Consignee").FontSize(7).FontColor(TextMuted);
                    c.Item().Text(details.ConsigneeName).Bold();
                    foreach (var line in details.ConsigneeAddressLines) c.Item().Text(line).FontSize(8);
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("L/C No.").FontSize(7).FontColor(TextMuted);
                    c.Item().Text($"{details.LcNumber} of {details.LcDate}");
                    c.Item().PaddingTop(4).Text("Bank").FontSize(7).FontColor(TextMuted);
                    c.Item().Text(details.IssuingBankName);
                });
            });
            column.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Notify Party").FontSize(7).FontColor(TextMuted);
                    c.Item().Text(details.NotifyPartyName).Bold();
                    foreach (var line in details.NotifyPartyAddressLines) c.Item().Text(line).FontSize(8);
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Remarks").FontSize(7).FontColor(TextMuted);
                    c.Item().Text(details.Remark1 ?? "");
                    c.Item().Text(details.Remark2 ?? "");
                    c.Item().Text(details.Remark3 ?? "");
                });
            });
            column.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text($"Port of Loading: {details.LoadPortDescription}");
                row.RelativeItem().Text($"Destination: {details.DestinationDescription}");
                row.RelativeItem().Text($"Carrier: {details.CarrierCode} / Ship Date: {details.ShipDate}");
            });

            if (!string.IsNullOrWhiteSpace(details.Detail))
            {
                column.Item().PaddingTop(8).Text("Detail").FontSize(7).FontColor(TextMuted);
                column.Item().Text(details.Detail).FontSize(8.5f);
            }
            column.Item().PaddingTop(8);
        }

        public static byte[] GenerateCartonsPdf(PackingListPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    PageNumberFooter(page);
                    page.Content().Column(column =>
                    {
                        HeaderBlock(column, details, "PACKING LIST (CARTONS)");

                        if (details.CartonGroups.Count == 0)
                        {
                            column.Item().Text("No carton breakdown entered.").FontColor(TextMuted);
                            return;
                        }

                        column.Item().Border(1).BorderColor(Copper).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(60);
                                foreach (var _ in details.Sizes) columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Padding(3).Text("Carton From").FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).Text("Carton To").FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).Text("Color").FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).Text("No. Ctns").FontColor(TextMuted).FontSize(7);
                                foreach (var size in details.Sizes)
                                    header.Cell().Padding(3).AlignRight().Text(size).FontColor(TextMuted).FontSize(7);
                            });

                            var grandTotal = 0m;
                            foreach (var group in details.CartonGroups)
                            {
                                table.Cell().Padding(3).Text(group.FromCartonNo.ToString());
                                table.Cell().Padding(3).Text(group.ToCartonNo.ToString());
                                table.Cell().Padding(3).Text(group.Color);
                                table.Cell().Padding(3).Text(group.NoOfCartons.ToString());
                                foreach (var size in details.Sizes)
                                {
                                    var qty = group.QtyBySize.GetValueOrDefault(size, 0);
                                    grandTotal += qty * Math.Max(group.NoOfCartons, 1);
                                    table.Cell().Padding(3).AlignRight().Text(qty > 0 ? qty.ToString("N0") : "-");
                                }
                            }

                            table.Cell().ColumnSpan((uint)(4 + details.Sizes.Count - 1)).AlignRight().Padding(3)
                                .Text("TOTAL PCS").Bold();
                            table.Cell().Padding(3).AlignRight().Text(grandTotal.ToString("N0")).Bold();
                        });
                    });
                });
            }).GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }

        public static byte[] GenerateHangingGarmentsPdf(PackingListPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    PageNumberFooter(page);
                    page.Content().Column(column =>
                    {
                        HeaderBlock(column, details, "PACKING LIST (HANGING GARMENTS)");

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Style: {details.StyleCode}");
                            row.RelativeItem().Text($"P.O. No.: {details.NewOrder}");
                        });
                        column.Item().PaddingTop(4);

                        if (details.StringGroups.Count == 0)
                        {
                            column.Item().Text("No hanging garment breakdown entered.").FontColor(TextMuted);
                            return;
                        }

                        column.Item().Border(1).BorderColor(Copper).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(60);
                                foreach (var _ in details.Sizes) columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(60);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Padding(3).Text("Bar").FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).Text("Stng. No.").FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).Text("Colour").FontColor(TextMuted).FontSize(7);
                                foreach (var size in details.Sizes)
                                    header.Cell().Padding(3).AlignRight().Text(size).FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).AlignRight().Text("Pcs./Stng.").FontColor(TextMuted).FontSize(7);
                                header.Cell().Padding(3).AlignRight().Text("Pcs./Bar").FontColor(TextMuted).FontSize(7);
                            });

                            var grandTotal = 0m;
                            var barTotals = new Dictionary<int, decimal>();
                            foreach (var group in details.StringGroups)
                            {
                                var rangeCount = (group.ToStringNo - group.FromStringNo) + 1;
                                var stringTotal = details.Sizes.Sum(s => group.QtyBySize.GetValueOrDefault(s, 0));
                                var barTotal = stringTotal * Math.Max(rangeCount, 1);
                                barTotals[group.BarNo] = barTotals.GetValueOrDefault(group.BarNo, 0) + barTotal;
                                grandTotal += barTotal;

                                table.Cell().Padding(3).Text(group.BarNo.ToString());
                                table.Cell().Padding(3).Text($"{group.FromStringNo}-{group.ToStringNo}");
                                table.Cell().Padding(3).Text(group.Color);
                                foreach (var size in details.Sizes)
                                {
                                    var qty = group.QtyBySize.GetValueOrDefault(size, 0);
                                    table.Cell().Padding(3).AlignRight().Text(qty > 0 ? qty.ToString("N0") : "-");
                                }
                                table.Cell().Padding(3).AlignRight().Text(stringTotal.ToString("N0"));
                                table.Cell().Padding(3).AlignRight().Text(barTotals[group.BarNo].ToString("N0"));
                            }

                            table.Cell().ColumnSpan((uint)(3 + details.Sizes.Count)).AlignRight().Padding(3)
                                .Text("TOTAL PCS").Bold();
                            table.Cell().ColumnSpan(2).Padding(3).AlignRight().Text(grandTotal.ToString("N0")).Bold();
                        });
                    });
                });
            }).GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
