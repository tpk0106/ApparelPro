using System.Reflection;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Redraws the real, current-day Sri Lanka Customs "Goods Declaration"
    // (CUSDEC I / Schedule II, Customs-53) and its "Continuation Sheet"
    // (CUSDEC II) - both supplied by the user as real pre-printed forms,
    // and both a different, more modern box layout (numbered 1-54, ASYCUDA
    // style) than the legacy DOS entry screen (IE_CUSD1.PRG) ever used.
    // Same "digital overlay" approach as Commercial Invoice/Certificate of
    // Origin - fully re-themed (dark background, white text, copper box
    // borders), not a literal print onto physical stationery.
    //
    // Several boxes on the real form have no matching field anywhere in the
    // legacy data or our migrated schema (TINs, a separate Customs
    // Reference Number, Trading Country, C.A.P., Nature of Transaction,
    // Preference, V.M., Adjustment) - drawn but left blank rather than
    // guessed at. Quota (box 39) is deliberately left blank too, per the
    // quota-is-industry-dead decision elsewhere in this project.
    //
    // CUSDEC I = header + the first item line. Every remaining item line
    // gets its own CUSDEC II continuation page, 3 items per page - matches
    // legacy IE_CUSD2.PRG's prt_importcus loop (one CUSDEC-II page per
    // item after the first).
    public static class CustomsDeclarationPrintEngine
    {
        private const string PageBg = "#0A0E14";
        private const string TextWhite = "#F4F6F8";
        private const string TextMuted = "#8B93A1";
        private const string Copper = "#C9803D";

        // Cropped from the real Sri Lanka Customs logo (SVG the user supplied,
        // rasterized and auto-cropped to its opaque bounding box, transparent
        // background) and embedded as a resource, same approach as the
        // Ceylon Chamber of Commerce logo on Certificate of Origin.
        private static readonly Lazy<byte[]> CustomsLogoBytes = new(() =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "apparelPro.BusinessLogic.Reports.ImportExport.Assets.sri-lanka-customs-logo.png";
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        });

        private static void ConfigureDarkPage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.0f, Unit.Centimetre);
            page.PageColor(PageBg);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(7.5f).FontColor(TextWhite));

            // Faint centered watermark of the real Sri Lanka Customs logo,
            // behind the content layer (QuestPDF layers page.Background()
            // beneath page.Content() automatically). The faintness is baked
            // into the embedded PNG's own alpha channel (~10% of full),
            // not applied via a QuestPDF opacity call.
            page.Background().AlignCenter().AlignMiddle()
                .Width(320).Image(CustomsLogoBytes.Value).FitArea();
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

        // One numbered form box - a small caption line plus the value,
        // bordered like the real form's ruled boxes.
        private static void Box(RowDescriptor row, string caption, string? value, float size = 1)
        {
            row.RelativeItem(size).Border(1).BorderColor(Copper).Padding(4).MinHeight(30).Column(c =>
            {
                c.Item().Text(caption).FontSize(6.5f).FontColor(TextMuted);
                c.Item().PaddingTop(2).Text(value ?? "").FontSize(8);
            });
        }

        private static void BoxColumn(ColumnDescriptor column, string caption, string? value)
        {
            column.Item().Border(1).BorderColor(Copper).Padding(4).MinHeight(30).Column(c =>
            {
                c.Item().Text(caption).FontSize(6.5f).FontColor(TextMuted);
                c.Item().PaddingTop(2).Text(value ?? "").FontSize(8);
            });
        }

        private static string DisplayOrCode(string? code, string? description) =>
            string.IsNullOrWhiteSpace(code) ? "" : (string.IsNullOrWhiteSpace(description) ? code : $"{code} - {description}");

        public static byte[] GenerateCusdecPdf(CustomsDeclarationPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var header = details.Header;
            var lines = details.Lines;
            var firstLine = lines.FirstOrDefault();
            var remainingLines = lines.Skip(1).ToList();

            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigureDarkPage(page);
                    PageNumberFooter(page);
                    page.Content().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("CUSDEC I").Bold().FontSize(11).FontColor(Copper);
                            row.RelativeItem(3).AlignCenter().Text("SCHEDULE II - SRI LANKA CUSTOMS - GOODS DECLARATION")
                                .Bold().FontSize(10);
                            row.RelativeItem().AlignRight().Text("Customs - 53").FontColor(TextMuted);
                        });
                        column.Item().PaddingBottom(4);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                BoxColumn(c, "2 EXPORTER (TIN: )", DisplayOrCode(header.ExporterCode, details.ExporterName));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                BoxColumn(c, "1 DECLARATION - Customs Reference Number", header.CusNo);
                                BoxColumn(c, "3 Pages / 4 List", $"{1 + (int)Math.Ceiling(remainingLines.Count / 3.0)}");
                                BoxColumn(c, "5 Items / 6 Total Packages / 7 Declarant's Sequence Number", $"{lines.Count}");
                            });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                BoxColumn(c, "8 CONSIGNEE (TIN: )", DisplayOrCode(header.ConsigneeCode, details.ConsigneeName));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                BoxColumn(c, "9 Person Responsible for Financial Settlement (TIN: )", "");
                            });
                        });

                        column.Item().Row(row =>
                        {
                            Box(row, "10 Cty of Last Cons/First Dst.", details.CountryOfConsignmentName);
                            Box(row, "11 Trading Cty.", "");
                            Box(row, "12 Value Details", "");
                            Box(row, "13 C.A.P.", "");
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                BoxColumn(c, "14 DECLARANT/REPRESENTATIVE (TIN: )", DisplayOrCode(header.DeclarantCode, details.DeclarantBuyerName));
                            });
                            row.RelativeItem().Column(c =>
                            {
                                BoxColumn(c, "15 Country of Export / 15 Cty. Ex. Code", "");
                                BoxColumn(c, "17A Cty. Dst. Code", header.CountryOfDestinationCode);
                            });
                        });

                        column.Item().Row(row =>
                        {
                            Box(row, "16 Country of origin", DisplayOrCode(header.CountryOfOriginCode, details.CountryOfOriginName));
                            Box(row, "17 Country of destination", DisplayOrCode(header.CountryOfDestinationCode, details.CountryOfDestinationName));
                        });

                        column.Item().Row(row =>
                        {
                            Box(row, "18 Vessel/Flight", header.Vessel);
                            Box(row, "19 Ctr.", DisplayOrCode(header.TransportModeCode, details.TransportModeDescription));
                            Box(row, "20 Delivery Terms", DisplayOrCode(header.DeliveryTermCode, details.DeliveryTermDescription), 2);
                        });

                        column.Item().Row(row =>
                        {
                            Box(row, "21 Voyage No./Date", $"{header.VoyageNo} {header.VoyageDate:dd/MM/yyyy}");
                            Box(row, "22 Currency & Total Amount Invoiced", firstLine == null ? "" : $"{firstLine.CurrencyCode} {lines.Sum(l => l.Fob ?? 0):N2}");
                            Box(row, "23 Exchange Rate", firstLine?.ExchangeRate?.ToString("N4"));
                            Box(row, "24 Nature of Transt.", "");
                        });

                        column.Item().Row(row =>
                        {
                            Box(row, "25 Mode Trans. at Border / 26 Inland Mode Transport", DisplayOrCode(header.TransportModeCode, details.TransportModeDescription));
                            Box(row, "27 Place of Loading/Discharging", $"{details.PortOfLoadingName} / {details.PortOfDischargeName}", 2);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                BoxColumn(c, "29 Office of Entry/Exit", $"{DisplayOrCode(header.ClearanceOfficeCode, details.ClearanceOfficeDescription)} / {DisplayOrCode(header.FrontierOfficeCode, details.FrontierOfficeDescription)}");
                                BoxColumn(c, "30 Location of Goods", header.LocationOfGoods);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                BoxColumn(c, "28 Financial and Banking Data - Bank Code / Terms of payment",
                                    $"{header.BankCode} {details.BankName} / {DisplayOrCode(header.PaymentTermCode, details.PaymentTermDescription)}");
                                BoxColumn(c, "Ref. No.", header.ReferenceNo);
                            });
                        });

                        column.Item().PaddingTop(6);

                        if (firstLine != null)
                        {
                            ItemBlock(column, firstLine, details);
                        }

                        if (details.AttachedDocuments.Count > 0)
                        {
                            column.Item().PaddingTop(4).Text("Attached Documents").FontSize(7.5f).FontColor(TextMuted);
                            foreach (var doc in details.AttachedDocuments)
                            {
                                // Matches legacy IE_CUSD2.PRG exactly: doc_no alone when
                                // doc_type is "." or blank, else "doc_no(doc_type)".
                                var label = string.IsNullOrWhiteSpace(doc.DocTypeCode) || doc.DocTypeCode == "."
                                    ? doc.DocNo
                                    : $"{doc.DocNo}({doc.DocTypeCode})";
                                column.Item().Text(label).FontSize(8);
                            }
                        }

                        column.Item().PaddingTop(4).Row(row =>
                        {
                            Box(row, "48 A.C. Number", header.PrepaymentAccountNo);
                            Box(row, "49 Identification of warehouse", $"{header.WarehouseNo} {header.WarehousePeriod}", 2);
                        });

                        column.Item().PaddingTop(4).Row(row =>
                        {
                            Box(row, "50", header.Remark1);
                            Box(row, "51", header.Remark2);
                            Box(row, "52", header.Remark3);
                        });

                        column.Item().PaddingTop(6).Text(
                            "53. I do hereby confirm that all particulars entered by me or on my behalf in the CusDec and " +
                            "electronically transferred to the Sri Lanka Customs are true & correct, and that the same " +
                            "particulars appear in print form on this document.").FontSize(7.5f).FontColor(TextMuted);
                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text($"Signature: {header.DeclarantName}").FontSize(8);
                            row.RelativeItem().AlignRight().Text($"54. Declaration submitted by: {header.SubmittedByName}").FontSize(8);
                        });
                    });
                });

                var groups = remainingLines
                    .Select((line, index) => new { line, index })
                    .GroupBy(x => x.index / 3)
                    .Select(g => g.Select(x => x.line).ToList())
                    .ToList();

                foreach (var group in groups)
                {
                    container.Page(page =>
                    {
                        ConfigureDarkPage(page);
                        PageNumberFooter(page);
                        page.Content().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("CUSDEC II").Bold().FontSize(11).FontColor(Copper);
                                row.RelativeItem(3).AlignCenter().Text("SRI LANKA CUSTOMS - GOODS DECLARATION - CONTINUATION SHEET")
                                    .Bold().FontSize(9);
                                row.RelativeItem().AlignRight().Text("Customs - 53").FontColor(TextMuted);
                            });
                            column.Item().Row(row =>
                            {
                                Box(row, "Consignee/Exporter", $"{details.ExporterName} / {details.ConsigneeName}", 3);
                                Box(row, "1 Declaration - Customs Ref", header.CusNo);
                            });
                            column.Item().PaddingBottom(6);

                            foreach (var line in group)
                            {
                                ItemBlock(column, line, details);
                                column.Item().PaddingBottom(4);
                            }

                            column.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text("Total").Bold();
                                row.RelativeItem(2).AlignRight().Text($"Summary of Taxes: {group.SelectMany(l => l.Taxes).Sum(t => t.Payable ?? 0):N2}").Bold();
                                row.RelativeItem().AlignRight().Text("Signature and Date: ___________");
                            });
                        });
                    });
                }
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        // Boxes 31-46 (packages/description, commodity, weights, procedure,
        // UOM/qty, price) plus box 47's tax calculation grid for one item.
        private static void ItemBlock(ColumnDescriptor column, CustomsDeclarationLineServiceModel line, CustomsDeclarationPrintDetailsServiceModel details)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().Border(1).BorderColor(Copper).Padding(4).MinHeight(60).Column(cc =>
                    {
                        cc.Item().Text("31 Marks and numbers - Containers No(s) - Number and kind / Description of Goods")
                            .FontSize(6.5f).FontColor(TextMuted);
                        cc.Item().PaddingTop(2).Text(line.Detail ?? "").FontSize(8);
                    });
                    BoxColumn(c, "44 Add. Info - Licence No", line.LicenceNo);
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Row(r2 =>
                    {
                        Box(r2, "32 Item No.", line.Item);
                        Box(r2, "33 Commodity (HS) Code", DisplayOrCode(line.CommodityCode, details.CommodityDescriptions.GetValueOrDefault(line.CommodityCode ?? "")));
                    });
                    c.Item().Row(r2 =>
                    {
                        Box(r2, "34 Cty. Orig. Code", DisplayOrCode(line.CountryCode, details.CountryNames.GetValueOrDefault(line.CountryCode ?? "")));
                        Box(r2, "35 Gross Mass (Kg)", line.GrossWeight?.ToString("N2"));
                        Box(r2, "36 Preference", "");
                    });
                    c.Item().Row(r2 =>
                    {
                        Box(r2, "37 Procedure", DisplayOrCode(line.CustomsProcedureCode, details.CustomsProcedureDescriptions.GetValueOrDefault(line.CustomsProcedureCode ?? "")));
                        Box(r2, "38 Net Mass (Kg)", line.NetWeight?.ToString("N2"));
                        Box(r2, "39 Quota", "");
                    });
                    c.Item().Row(r2 =>
                    {
                        Box(r2, "41 UOM & Qty.", $"{DisplayOrCode(line.SupplementaryUnitCode, details.UnitDescriptions.GetValueOrDefault(line.SupplementaryUnitCode ?? ""))} {line.SupplementaryQty:N2}", 2);
                        Box(r2, "42 Item Price (FOB/CIF)", line.Fob?.ToString("N2"));
                    });
                    c.Item().Row(r2 =>
                    {
                        Box(r2, "43 V.M.", "");
                        Box(r2, "45 Adjustment", "");
                        Box(r2, "46 Statistical Value", line.Value);
                    });
                });
            });

            if (line.Taxes.Count > 0)
            {
                column.Item().PaddingTop(2).Border(1).BorderColor(Copper).Padding(4).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    table.Header(headerRow =>
                    {
                        headerRow.Cell().Text("47 Type").FontColor(TextMuted).FontSize(6.5f);
                        headerRow.Cell().Text("Tax Base").FontColor(TextMuted).FontSize(6.5f);
                        headerRow.Cell().Text("Rate").FontColor(TextMuted).FontSize(6.5f);
                        headerRow.Cell().Text("Amount").FontColor(TextMuted).FontSize(6.5f);
                        headerRow.Cell().Text("MP").FontColor(TextMuted).FontSize(6.5f);
                    });
                    foreach (var tax in line.Taxes)
                    {
                        table.Cell().Text(DisplayOrCode(tax.TaxCode, details.TaxDescriptions.GetValueOrDefault(tax.TaxCode ?? "")));
                        table.Cell().Text(DisplayOrCode(tax.BaseCode, details.TaxBaseDescriptions.GetValueOrDefault(tax.BaseCode ?? "")));
                        table.Cell().Text(tax.Rate?.ToString("N1"));
                        table.Cell().Text(tax.Amount?.ToString("N2"));
                        table.Cell().Text("");
                    }
                    table.Cell().ColumnSpan(2).AlignRight().Text($"Total Item ({line.Item})").Bold();
                    table.Cell().Text("");
                    table.Cell().Text(line.Taxes.Sum(t => t.Payable ?? 0).ToString("N2")).Bold();
                    table.Cell().Text("");
                });
            }
        }
    }
}
