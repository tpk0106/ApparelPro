using apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Redraws the real Sri Lanka Customs e-CDN "Boat Note for Goods Passed
    // out of Customs Control" sample the user supplied - a modern digital
    // release document, not a physical pre-printed form, so this is a fully
    // re-themed digital document (dark background, copper section headers)
    // rather than an overlay onto stationery, same approach as the other IE
    // print engines.
    public static class BoatNotePrintEngine
    {
        private const string PageBg = "#0A0E14";
        private const string TextWhite = "#F4F6F8";
        private const string TextMuted = "#8B93A1";
        private const string Copper = "#C9803D";

        private static void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.2f, Unit.Centimetre);
            page.PageColor(PageBg);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9).FontColor(TextWhite));
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

        private static void SectionHeader(ColumnDescriptor column, string label) =>
            column.Item().PaddingTop(10).PaddingBottom(4).Text($"[ {label} ]").Bold().FontColor(Copper).FontSize(9.5f);

        private static void FieldPair(ColumnDescriptor column, string label1, string value1, string label2, string value2)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span($"{label1}: ").FontColor(TextMuted);
                    t.Span(value1 ?? "");
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span($"{label2}: ").FontColor(TextMuted);
                    t.Span(value2 ?? "");
                });
            });
        }

        public static byte[] GeneratePdf(BoatNotePrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var header = details.Header;

            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    PageNumberFooter(page);
                    page.Content().Column(column =>
                    {
                        column.Item().AlignCenter().Text("SRI LANKA CUSTOMS DEPARTMENT").Bold().FontSize(13).FontColor(Copper);
                        column.Item().AlignCenter().PaddingBottom(6)
                            .Text("BOAT NOTE FOR GOODS PASSED OUT OF CUSTOMS CONTROL (e-CDN)").FontSize(9.5f).FontColor(TextMuted);
                        column.Item().LineHorizontal(0.5f).LineColor(Copper);

                        column.Item().PaddingTop(6);
                        FieldPair(column, "Boat Note No", header.BoatNoteNumber ?? "", "Date/Time",
                            header.BoatNoteDateTime?.ToString("dd/MM/yyyy HH:mm") ?? "");
                        FieldPair(column, "Customs Reg No", header.CustomsRegNo ?? "", "ASYCUDA CusDec Ref", header.CusDecRef ?? "");

                        SectionHeader(column, "VESSEL & VOYAGE DETAILS");
                        FieldPair(column, "M/Vessel Name", header.VesselName ?? "", "Voyage No", header.VoyageNo ?? "");
                        FieldPair(column, "Port of Loading", details.PortOfLoadingDescription, "Discharge Port", details.DischargePortDescription);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Shipper: ").FontColor(TextMuted);
                                t.Span(details.ShipperName);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t =>
                                {
                                    t.Span("Consignee: ").FontColor(TextMuted);
                                    t.Span(details.ConsigneeName);
                                });
                                foreach (var line in details.ConsigneeAddressLines)
                                    c.Item().Text(line).FontSize(8).FontColor(TextWhite);
                            });
                        });

                        SectionHeader(column, "CARGO SUMMARY");
                        if (details.Lines.Count == 0)
                        {
                            column.Item().Text("No cargo lines entered.").FontColor(TextMuted);
                        }
                        else
                        {
                            column.Item().Border(1).BorderColor(Copper).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.3f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1f);
                                    columns.RelativeColumn(2f);
                                    columns.RelativeColumn(1f);
                                    columns.RelativeColumn(1.1f);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Padding(3).Text("Container No.").FontColor(TextMuted).FontSize(7.5f);
                                    h.Cell().Padding(3).Text("Seal No.").FontColor(TextMuted).FontSize(7.5f);
                                    h.Cell().Padding(3).Text("No. of Pkgs.").FontColor(TextMuted).FontSize(7.5f);
                                    h.Cell().Padding(3).Text("Description of Goods").FontColor(TextMuted).FontSize(7.5f);
                                    h.Cell().Padding(3).Text("HS Code").FontColor(TextMuted).FontSize(7.5f);
                                    h.Cell().Padding(3).AlignRight().Text("Gross Wt.").FontColor(TextMuted).FontSize(7.5f);
                                });

                                var totalWeight = 0m;
                                foreach (var line in details.Lines)
                                {
                                    totalWeight += line.GrossWeight;
                                    table.Cell().Padding(3).Text(line.ContainerNo);
                                    table.Cell().Padding(3).Text(line.SealNo ?? "");
                                    table.Cell().Padding(3).Text(line.PackageQuantity);
                                    table.Cell().Padding(3).Text(line.Description);
                                    table.Cell().Padding(3).Text(line.HsCode ?? "");
                                    table.Cell().Padding(3).AlignRight()
                                        .Text($"{line.GrossWeight:N2} {line.WeightUnit}");
                                }

                                table.Cell().ColumnSpan(5).AlignRight().Padding(3).Text("TOTAL").Bold();
                                table.Cell().Padding(3).AlignRight().Text($"{totalWeight:N2}").Bold();
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(header.Remarks))
                        {
                            column.Item().PaddingTop(8).Text("Remarks").FontSize(7.5f).FontColor(TextMuted);
                            column.Item().Text(header.Remarks).FontSize(8.5f);
                        }

                        SectionHeader(column, "DIGITAL VERIFICATIONS & RELEASES");
                        column.Item().Text(t =>
                        {
                            t.Span("Customs Officer Status: ").FontColor(TextMuted);
                            t.Span(header.CustomsOfficerStatus ?? "-").Bold();
                            if (!string.IsNullOrWhiteSpace(header.CustomsOfficerReference))
                                t.Span($"  (ID: {header.CustomsOfficerReference})").FontColor(TextMuted).FontSize(8);
                        });
                        column.Item().Text(t =>
                        {
                            t.Span("Terminal Operator Release: ").FontColor(TextMuted);
                            t.Span(header.TerminalOperatorReleaseStatus ?? "-").Bold();
                            if (!string.IsNullOrWhiteSpace(header.TerminalOperatorReference))
                                t.Span($"  ({header.TerminalOperatorReference})").FontColor(TextMuted).FontSize(8);
                        });
                        column.Item().Text(t =>
                        {
                            t.Span("Shipper/Agent Sign-off: ").FontColor(TextMuted);
                            t.Span(header.ShipperAgentSignOffStatus ?? "-").Bold();
                            if (!string.IsNullOrWhiteSpace(header.ChaLicenseNo))
                                t.Span($"  (CHA License: {header.ChaLicenseNo})").FontColor(TextMuted).FontSize(8);
                        });
                    });
                });
            }).GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
