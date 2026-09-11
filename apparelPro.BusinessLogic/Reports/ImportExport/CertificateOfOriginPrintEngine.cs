using System.Reflection;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Both formats are fully re-themed digital documents (dark background,
    // white text, copper borders) - not a literal overlay onto physical
    // pre-printed stationery, same "digital overlay" approach as
    // CommercialInvoicePrintEngine. "Full" is our own internal layout;
    // "Chamber" redraws the real Ceylon Chamber of Commerce "Certificate of
    // Origin" (EXP 5) form's numbered box layout (boxes 1-13, 15-16) - boxes
    // 6 (For Official Use) and 17 (QR Code) are chamber-side, drawn blank.
    public static class CertificateOfOriginPrintEngine
    {
        private const string PageBg = "#0A0E14";
        private const string TextWhite = "#F4F6F8";
        private const string TextMuted = "#8B93A1";
        private const string Copper = "#C9803D";

        // Cropped from the chamber's own EXP 5 PDF (white background made
        // transparent) and embedded as a resource so the print engine has no
        // runtime file-path dependency. See Reports/ImportExport/Assets/.
        private static readonly Lazy<byte[]> ChamberLogoBytes = new(() =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "apparelPro.BusinessLogic.Reports.ImportExport.Assets.ceylon-chamber-logo.png";
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        });

        private static void ConfigureDarkPage(PageDescriptor page)
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

        private static void BoxHeader(ColumnDescriptor column, string label) =>
            column.Item().Text(label).Bold().FontColor(Copper).FontSize(8);

        // Full Format - our own internal layout, not fitted to any specific
        // real form's box proportions.
        public static byte[] GenerateFullFormatPdf(CertificateOfOriginPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var header = details.Header;

            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigureDarkPage(page);

                    page.Content().Column(column =>
                    {
                        column.Item().AlignCenter().Text("CERTIFICATE OF ORIGIN").Bold().FontColor(Copper).FontSize(16);
                        column.Item().PaddingTop(2).AlignCenter().Text($"Invoice No.: {header.InvoiceNumber}    Ref. No.: {header.RefNo}").FontSize(9);

                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                BoxHeader(c, "CONSIGNOR / EXPORTER");
                                c.Item().Text(details.ExporterCompanyName);
                                c.Item().Text(details.ExporterAddress1);
                                c.Item().Text(details.ExporterAddress2);
                                c.Item().Text(details.ExporterCityPostCodeCountry);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                BoxHeader(c, "CONSIGNEE");
                                c.Item().Text(details.ConsigneeName);
                                foreach (var line in details.ConsigneeAddressLines) c.Item().Text(line);
                            });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Text(t =>
                            {
                                t.Span("Country of Origin: ").FontColor(Copper);
                                t.Span(header.CountryOfOrigin);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Text(t =>
                            {
                                t.Span("Port of Loading: ").FontColor(Copper);
                                t.Span(header.PortOfLoading ?? "");
                            });
                        });

                        column.Item().PaddingTop(6).Border(1).BorderColor(Copper).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // Item No
                                columns.RelativeColumn(2);   // Shipping Marks
                                columns.RelativeColumn(2);   // Package Type/Qty
                                columns.RelativeColumn(3);   // Item Name
                                columns.RelativeColumn(2);   // HS Code
                                columns.RelativeColumn(2);   // Nett Weight
                                columns.RelativeColumn(2);   // Gross Weight
                            });
                            table.Header(h =>
                            {
                                foreach (var label in new[]
                                {
                                    "Item No.", "Shipping Marks", "Package Type/Qty.", "Item Name",
                                    "H.S. Code", "Nett Weight", "Gross Weight",
                                })
                                    h.Cell().Text(label).Bold().FontColor(Copper).FontSize(7);
                                h.Cell().ColumnSpan(7).PaddingTop(2).LineHorizontal(1).LineColor(Copper);
                            });
                            foreach (var line in details.Lines)
                            {
                                table.Cell().Text(line.ItemNo.ToString()).FontSize(7);
                                table.Cell().Text(line.ShippingMarks).FontSize(7);
                                table.Cell().Text(line.PackageTypeQuantity).FontSize(7);
                                table.Cell().Text(line.ItemName).FontSize(7);
                                table.Cell().Text(line.HsCode).FontSize(7);
                                table.Cell().Text(line.NettWeight.ToString("N2")).FontSize(7);
                                table.Cell().Text(line.GrossWeight.ToString("N2")).FontSize(7);
                            }
                        });

                        if (!string.IsNullOrWhiteSpace(header.OtherRemarks))
                        {
                            column.Item().PaddingTop(6).Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                BoxHeader(c, "OTHER REMARKS");
                                c.Item().Text(header.OtherRemarks).FontSize(8);
                            });
                        }

                        column.Item().PaddingTop(10).Text(
                            $"The undersigned declares that the goods described above originate in the country as shown ({header.CountryOfOrigin}).")
                            .FontSize(8);

                        column.Item().PaddingTop(16).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                BoxHeader(c, "COMPETENT AUTHORITY");
                                c.Item().Text(header.CompetentAuthorityName ?? "");
                                c.Item().Text($"{header.IssuePlace ?? ""}  {header.IssueDate:dd/MM/yyyy}").FontSize(8).FontColor(TextMuted);
                                c.Item().PaddingTop(20).Width(150).LineHorizontal(1).LineColor(Copper);
                                c.Item().Text("Signature").FontSize(7).FontColor(TextMuted);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                BoxHeader(c, "REQUEST SUBMITTED BY");
                                c.Item().Text(header.RequestSubmittedBy ?? "");
                                c.Item().PaddingTop(30).Width(150).LineHorizontal(1).LineColor(Copper);
                                c.Item().Text("Signature").FontSize(7).FontColor(TextMuted);
                            });
                        });
                    });

                    PageNumberFooter(page);
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        // Pre-printed - Ceylon Chamber of Commerce "Certificate of Origin"
        // (EXP 5) form supplied 2026-09-11. Redraws the real form's numbered
        // boxes 1-13/15-16 top-to-bottom/left-to-right; 6 (For Official Use)
        // and 17 (QR Code) are chamber-issued, drawn blank.
        public static byte[] GenerateChamberFormatPdf(CertificateOfOriginPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var header = details.Header;

            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigureDarkPage(page);

                    page.Content().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                c.Item().Border(1).BorderColor(Copper).Padding(6).Height(70).Column(cc =>
                                {
                                    BoxHeader(cc, "1. Consignor/Exporter");
                                    cc.Item().Text(details.ExporterCompanyName);
                                    cc.Item().Text(details.ExporterAddress1);
                                    cc.Item().Text(details.ExporterAddress2);
                                    cc.Item().Text(details.ExporterCityPostCodeCountry);
                                });
                                c.Item().Border(1).BorderColor(Copper).Padding(6).Height(70).Column(cc =>
                                {
                                    BoxHeader(cc, "3. Consignee");
                                    cc.Item().Text(details.ConsigneeName);
                                    foreach (var line in details.ConsigneeAddressLines) cc.Item().Text(line);
                                });
                                c.Item().Border(1).BorderColor(Copper).Padding(6).Height(40).Column(cc =>
                                {
                                    BoxHeader(cc, "5. Port of Loading");
                                    cc.Item().Text(header.PortOfLoading ?? "");
                                });
                            });
                            row.RelativeItem(2).Column(c =>
                            {
                                c.Item().Border(1).BorderColor(Copper).Padding(6).Column(cc =>
                                {
                                    BoxHeader(cc, "2. Ref. No.");
                                    cc.Item().Text(header.RefNo);
                                });
                                c.Item().PaddingVertical(4).AlignCenter().Text("Certificate of Origin").Bold().FontColor(Copper).FontSize(14);
                                // Supplied logo is a wide lockup (emblem + "The Ceylon Chamber of
                                // Commerce" wordmark side by side, transparent background) - sized
                                // by width so it isn't stretched, no separate text line needed.
                                c.Item().AlignCenter().Width(180).Image(ChamberLogoBytes.Value).FitWidth();
                                c.Item().PaddingTop(2).AlignCenter().Text("50, Navam Mawatha, Colombo 02, Sri Lanka.").FontSize(7).FontColor(TextMuted);
                                c.Item().AlignCenter().Text("Tel. (+94)11-5588800  |  Fax. (+94)11-2449352  |  Email. eco@chamber.lk").FontSize(7).FontColor(TextMuted);
                                c.Item().AlignCenter().Text("Web. www.edocs.lk").FontSize(7).FontColor(TextMuted);
                                c.Item().Border(1).BorderColor(Copper).Padding(6).Column(cc =>
                                {
                                    BoxHeader(cc, "4. Country of Origin");
                                    cc.Item().Text(header.CountryOfOrigin);
                                });
                                c.Item().Border(1).BorderColor(Copper).Padding(6).Height(40).Column(cc =>
                                {
                                    BoxHeader(cc, "6. For Official Use");
                                });
                            });
                        });

                        column.Item().PaddingTop(4).Border(1).BorderColor(Copper).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // 7. Item No
                                columns.RelativeColumn(2);   // 8. Shipping Marks
                                columns.RelativeColumn(2);   // 9. Package Type/Qty
                                columns.RelativeColumn(3);   // 10. Item Name
                                columns.RelativeColumn(2);   // 11. H.S. Code
                                columns.RelativeColumn(2);   // 12. Nett/Gross Weight
                            });
                            table.Header(h =>
                            {
                                foreach (var label in new[]
                                {
                                    "7. Item No.", "8. Shipping Marks", "9. Package Type/Quantity", "10. Item Name",
                                    "11. H.S. Code", "12. Nett Weight & Gross Weight",
                                })
                                    h.Cell().Text(label).Bold().FontColor(Copper).FontSize(7);
                                h.Cell().ColumnSpan(6).PaddingTop(2).LineHorizontal(1).LineColor(Copper);
                            });
                            foreach (var line in details.Lines)
                            {
                                table.Cell().Text(line.ItemNo.ToString()).FontSize(7);
                                table.Cell().Text(line.ShippingMarks).FontSize(7);
                                table.Cell().Text(line.PackageTypeQuantity).FontSize(7);
                                table.Cell().Text(line.ItemName).FontSize(7);
                                table.Cell().Text(line.HsCode).FontSize(7);
                                table.Cell().Text($"{line.NettWeight:N2} / {line.GrossWeight:N2}").FontSize(7);
                            }
                        });

                        column.Item().PaddingTop(4).Border(1).BorderColor(Copper).Padding(6).Height(50).Column(c =>
                        {
                            BoxHeader(c, "13. Other Remarks");
                            c.Item().Text(header.OtherRemarks ?? "").FontSize(8);
                        });

                        column.Item().PaddingTop(4).Border(1).BorderColor(Copper).Padding(6).Text(
                            $"14. The undersigned declares that the goods described above originate in the country as shown in box 4.")
                            .FontSize(8);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Height(90).Column(c =>
                            {
                                BoxHeader(c, "15. Competent Authority (name, signature, place and date of issue)");
                                c.Item().Text(header.CompetentAuthorityName ?? "");
                                c.Item().Text($"{header.IssuePlace ?? ""}  {header.IssueDate:dd/MM/yyyy}").FontSize(8).FontColor(TextMuted);
                                c.Item().PaddingTop(16).Text("Signed by").FontSize(7).FontColor(TextMuted);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Height(90).Column(c =>
                            {
                                BoxHeader(c, "16. Request submitted by");
                                c.Item().Text(header.RequestSubmittedBy ?? "");
                                c.Item().PaddingTop(30).Text("Signed by").FontSize(7).FontColor(TextMuted);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Height(90).Column(c =>
                            {
                                BoxHeader(c, "17. QR Code");
                            });
                        });
                    });

                    PageNumberFooter(page);
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
