using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Every Commercial Invoice print format is a fully re-themed digital
    // document (dark background, white text, copper borders/checkboxes) -
    // not a literal overlay onto physical pre-printed stationery. The two
    // "pre-printed" formats redraw the box layout of two real-world templates
    // the user supplied (a FedEx-style international commercial invoice, and
    // a DHL-style one used for Sri Lanka customs) rather than embedding those
    // PDFs as images, so we can style them and keep the data live/text-
    // selectable. Some boxes on both real forms have no matching field in our
    // schema yet (Payment Method, HS Code, per-line Unit Value, etc.) - see
    // project_commercial_invoice_print_extra_fields_todo memory - those boxes
    // are drawn but left blank rather than guessed at.
    //
    // Item tables render one row per actual CommercialInvoiceLine (real
    // Quantity/Unit/PackingMedia data), plus the free-text Detail memo as a
    // separate block underneath - legacy (ie_coin1.prg) never computed a
    // printed description from the structured shipment lines, it's hand-typed
    // by the merchandiser, so Detail is kept as its own text rather than
    // forced into a per-line "description" column.
    public static class CommercialInvoicePrintEngine
    {
        private const string PageBg = "#0A0E14";
        private const string PanelBg = "#141922";
        private const string TextWhite = "#F4F6F8";
        private const string TextMuted = "#8B93A1";
        private const string Copper = "#C9803D";

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

        // A small square, copper-filled when checked, copper-outlined when not -
        // stands in for the real form's tick-boxes (Payment Method, Terms of Sale).
        private static void CheckBox(ColumnDescriptor column, string label, bool isChecked)
        {
            column.Item().PaddingBottom(2).Row(row =>
            {
                row.ConstantItem(12).Height(12).Background(isChecked ? Copper : PageBg).Border(1).BorderColor(Copper);
                row.ConstantItem(6);
                row.RelativeItem().Text(label).FontSize(8);
            });
        }

        private static string JoinRemarks(CommercialInvoiceHeaderServiceModel header) =>
            string.Join(" ", new[] { header.Remark1, header.Remark2, header.Remark3 }.Where(s => !string.IsNullOrWhiteSpace(s)));

        // "1"/"2" are the Carton/Container codes the line-item entry form
        // saves (see commercial-invoice-list.component.tsx's Packing Media
        // dropdown) - this is genuinely "Type of Packaging" on both real forms.
        private static string PackagingTypeLabel(string? packingMedia) => packingMedia switch
        {
            "1" => "Carton",
            "2" => "Container",
            _ => "",
        };

        // Full Format - legacy ie_coin5.prg's own boxed layout (shipper/consignee
        // blocks, vessel/port row, item table, signature line).
        public static byte[] GenerateFullFormatPdf(CommercialInvoicePrintDetailsServiceModel details)
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
                        column.Item().Border(1).BorderColor(Copper).Background(PanelBg).Padding(8).Row(row =>
                        {
                            row.RelativeItem(2).Column(shipper =>
                            {
                                shipper.Item().Text("SHIPPER:").Bold();
                                shipper.Item().Text(details.ShipperCompanyName);
                                shipper.Item().Text(details.ShipperAddress1);
                                shipper.Item().Text(details.ShipperAddress2);
                                shipper.Item().Text(details.ShipperAddress3);
                            });
                            row.RelativeItem(1).Column(invoiceBox =>
                            {
                                invoiceBox.Item().Text("INVOICE NO").Bold().FontColor(Copper);
                                invoiceBox.Item().Text(header.InvoiceNumber);
                                invoiceBox.Item().PaddingTop(4).Text("DATE").Bold().FontColor(Copper);
                                invoiceBox.Item().Text(header.InvoiceDate.ToString("dd/MM/yyyy"));
                            });
                        });

                        column.Item().Border(1).BorderColor(Copper).Background(PanelBg).Padding(8).Row(row =>
                        {
                            row.RelativeItem(2).Column(consignee =>
                            {
                                consignee.Item().Text("CONSIGNEE:").Bold();
                                consignee.Item().Text(details.ConsigneeName);
                                foreach (var line in details.ConsigneeAddressLines)
                                    consignee.Item().Text(line);
                            });
                            row.RelativeItem(1).AlignCenter().AlignMiddle().Text("COMMERCIAL INVOICE").Bold().FontColor(Copper).FontSize(13);
                        });

                        column.Item().Border(1).BorderColor(Copper).Background(PanelBg).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Column(c => { c.Item().Text("VESSEL / FLT.").Bold().FontColor(Copper); c.Item().Text(header.CarrierCode ?? ""); });
                            row.RelativeItem().Column(c => { c.Item().Text("PORT OF LOADING").Bold().FontColor(Copper); c.Item().Text(details.LoadPortDescription); });
                            row.RelativeItem().Column(c => { c.Item().Text("PORT OF DISCHARGE").Bold().FontColor(Copper); c.Item().Text(details.DestinationDescription); });
                            row.RelativeItem().Column(c => { c.Item().Text("L/C NO.").Bold().FontColor(Copper); c.Item().Text(header.LcNumber ?? ""); });
                        });

                        column.Item().Border(1).BorderColor(Copper).Background(PanelBg).Padding(6).Column(c =>
                        {
                            c.Item().Text("MARKS & NOS / DESCRIPTION / QTY / UNIT / U-PRICE / AMOUNT").Bold().FontColor(Copper);
                            c.Item().PaddingTop(4).Text(header.Detail ?? "").FontFamily(Fonts.Consolas).FontSize(8);
                        });

                        column.Item().Border(1).BorderColor(Copper).Background(PanelBg).Padding(8).Column(c =>
                        {
                            c.Item().Text(JoinRemarks(header));
                        });

                        column.Item().PaddingTop(30).Text($"FOR {details.ShipperCompanyName}");
                        column.Item().PaddingTop(30).Width(150).LineHorizontal(1).LineColor(Copper);
                        column.Item().Text("IMPORT/EXPORT MANAGER");
                    });

                    PageNumberFooter(page);
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        // Pre-printed - International (FedEx-style template supplied 2026-09-08).
        // Redraws that form's own box layout: Date of Exportation / Shipper's
        // Export Reference row, Shipper / Consignee blocks, Country of Ultimate
        // Destination, item table, Payment Method + Terms of Sale checkboxes.
        public static byte[] GenerateFedExStylePdf(CommercialInvoicePrintDetailsServiceModel details, bool printAssessmentNo)
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
                        // Row order below follows the supplied FedEx-style template
                        // top-to-bottom exactly, including boxes we have no data
                        // for yet (Air Waybill No., Country of Export, Reason for
                        // Export, No. of Pkgs/Type of Packaging/Unit of Measure/
                        // Weight per line) - see project_commercial_invoice_print_
                        // extra_fields_todo memory. Rough proportions, not measured
                        // to the physical form - fine to nudge later.
                        column.Item().AlignCenter().Text("COMMERCIAL INVOICE").Bold().FontColor(Copper).FontSize(16);

                        column.Item().PaddingTop(6).Border(1).BorderColor(Copper).Padding(6).Text(t =>
                        {
                            t.Span("International Air Waybill No.: ").FontColor(Copper);
                            t.Span("");
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Date of Exportation: ").FontColor(Copper); t.Span(header.ShipDate?.ToString("dd/MM/yyyy") ?? ""); });
                            row.RelativeItem().Text(t => { t.Span("Shipper's Export References (order no., invoice no.): ").FontColor(Copper); t.Span(header.InvoiceNumber); });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("SHIPPER / EXPORTER").Bold().FontColor(Copper);
                                c.Item().Text(details.ShipperCompanyName);
                                c.Item().Text(details.ShipperAddress1);
                                c.Item().Text(details.ShipperAddress2);
                                c.Item().Text(details.ShipperAddress3);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("CONSIGNEE").Bold().FontColor(Copper);
                                c.Item().Text(details.ConsigneeName);
                                foreach (var line in details.ConsigneeAddressLines) c.Item().Text(line);
                            });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Text(t =>
                            {
                                t.Span("Country of Export: ").FontColor(Copper);
                                t.Span("");
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("IMPORTER - IF OTHER THAN CONSIGNEE").Bold().FontColor(Copper).FontSize(8);
                            });
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Text(t =>
                        {
                            t.Span("Reason for Export (e.g. personal gift, return for repair): ").FontColor(Copper);
                            t.Span("");
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Text(t =>
                        {
                            t.Span("Country of Ultimate Destination: ").FontColor(Copper);
                            t.Span(details.DestinationDescription);
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // Country of Origin
                                columns.RelativeColumn(1);   // Marks/No's
                                columns.RelativeColumn(1);   // No. of Pkgs
                                columns.RelativeColumn(1);   // Type of Packaging
                                columns.RelativeColumn(3);   // Full Description of Goods
                                columns.RelativeColumn(1);   // HS Code
                                columns.RelativeColumn(1);   // Qty
                                columns.RelativeColumn(1);   // Unit of Measure
                                columns.RelativeColumn(1);   // Weight
                                columns.RelativeColumn(1);   // Unit Value
                                columns.RelativeColumn(1);   // Total Value
                            });
                            table.Header(h =>
                            {
                                foreach (var label in new[]
                                {
                                    "Country of Origin", "Marks/No's", "No. of Pkgs", "Type of Packaging",
                                    "Full Description of Goods", "HS Code", "Qty.", "Unit of Measure", "Weight",
                                    "Unit Value", "Total Value",
                                })
                                    h.Cell().Text(label).Bold().FontColor(Copper).FontSize(7);
                                h.Cell().ColumnSpan(11).PaddingTop(2).LineHorizontal(1).LineColor(Copper);
                            });
                            // One row per shipment line - Qty/Unit of Measure/Type
                            // of Packaging come from the real per-line data
                            // (Quantity/Unit/PackingMedia); Country of Origin,
                            // Marks/No's, No. of Pkgs, HS Code, Weight, Unit
                            // Value and Total Value have no field yet (see
                            // project_commercial_invoice_print_extra_fields_todo).
                            foreach (var line in details.Lines)
                            {
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text(PackagingTypeLabel(line.PackingMedia)).FontSize(7);
                                table.Cell().Text($"{line.Order} {line.StyleCode} {line.NewOrder}").FontSize(7);
                                table.Cell().Text("");
                                table.Cell().Text(line.Quantity.ToString("N0")).FontSize(7);
                                table.Cell().Text(line.Unit).FontSize(7);
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text("");
                            }
                        });

                        if (!string.IsNullOrWhiteSpace(header.Detail))
                        {
                            column.Item().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Full Description of Goods (free text)").Bold().FontColor(Copper).FontSize(8);
                                c.Item().Text(header.Detail).FontFamily(Fonts.Consolas).FontSize(7);
                            });
                        }

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Total Pkgs: ").FontColor(Copper); t.Span(""); });
                            row.RelativeItem().Text(t => { t.Span("Total Weight: ").FontColor(Copper); t.Span(""); });
                            row.RelativeItem().Text(t => { t.Span("Currency: ").FontColor(Copper); t.Span(header.CurrencyCode ?? ""); });
                            row.RelativeItem().Text(t => { t.Span("Total Invoice Value: ").FontColor(Copper); t.Span(""); });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Payment Method").Bold().FontColor(Copper);
                                CheckBox(c, "L/C", !string.IsNullOrWhiteSpace(header.LcNumber));
                                CheckBox(c, "T/T", false);
                                CheckBox(c, "Others", false);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Check one (Terms)").Bold().FontColor(Copper);
                                var term = (header.TradeTermCode ?? "").Trim().ToUpperInvariant();
                                CheckBox(c, "F.O.B.", term == "FOB");
                                CheckBox(c, "C & F", term is "CFR" or "C&F" or "CNF");
                                CheckBox(c, "C.I.F.", term == "CIF");
                            });
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                        {
                            c.Item().Text("Additional Details (not on this form, kept for reference)").Bold().FontColor(Copper).FontSize(7);
                            c.Item().Text(t => { t.Span("Notify Party: ").FontColor(Copper); t.Span(details.NotifyPartyName); });
                            foreach (var line in details.NotifyPartyAddressLines) c.Item().Text(line);
                            c.Item().Text(t => { t.Span("Issuing Bank: ").FontColor(Copper); t.Span(details.IssuingBankName); });
                            c.Item().Text(t => { t.Span("L/C No / Date: ").FontColor(Copper); t.Span($"{header.LcNumber} / {header.LcDate:dd/MM/yyyy}"); });
                            if (printAssessmentNo)
                                c.Item().Text(t => { t.Span("Ass. No.: ").FontColor(Copper); t.Span(header.AssessmentNumber ?? ""); });
                            c.Item().Text(t => { t.Span("Port of Loading: ").FontColor(Copper); t.Span(details.LoadPortDescription); });
                            c.Item().Text(t => { t.Span("Carrier / Ship Date: ").FontColor(Copper); t.Span($"{header.CarrierCode} / {header.ShipDate:dd/MM/yyyy}"); });
                            c.Item().Text(t => { t.Span("Remarks: ").FontColor(Copper); t.Span(JoinRemarks(header)); });
                        });

                        column.Item().PaddingTop(10).Text("I DECLARE ALL THE INFORMATION CONTAINED IN THE INVOICE TO BE TRUE AND CORRECT.").FontSize(8);
                        column.Item().PaddingTop(24).Width(150).LineHorizontal(1).LineColor(Copper);
                        column.Item().Text("SIGNATURE OF SHIPPER/EXPORTER").FontSize(8).FontColor(TextMuted);
                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text("NAME (PLEASE PRINT)").FontSize(7).FontColor(TextMuted);
                            row.RelativeItem().Text("TITLE (PLEASE PRINT)").FontSize(7).FontColor(TextMuted);
                            row.RelativeItem().Text("DATE").FontSize(7).FontColor(TextMuted);
                        });
                    });

                    PageNumberFooter(page);
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        // Pre-printed - Sri Lanka Customs (DHL-style template supplied 2026-09-08).
        // Redraws Sender/Receiver/Delivery blocks, invoice info block, item
        // table, and totals/declaration/additional-info sections.
        public static byte[] GenerateSriLankaCustomsPdf(CommercialInvoicePrintDetailsServiceModel details, bool printAssessmentNo)
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
                        // Row order below follows the supplied DHL-style
                        // ("Sri Lanka customs") template top-to-bottom exactly,
                        // including boxes we have no data for yet (Shipment
                        // Number, Sender/Receiver VAT, Reason for Export, per-
                        // line HS code/weight/unit value, cost breakdown) - see
                        // project_commercial_invoice_print_extra_fields_todo
                        // memory. Rough proportions, not measured to the
                        // physical form - fine to nudge later.
                        column.Item().AlignCenter().Text("Commercial Invoice").Bold().FontColor(Copper).FontSize(16);

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Sender details").Bold().FontColor(Copper);
                                c.Item().Text(t => { t.Span("Company: ").FontColor(Copper); t.Span(details.ShipperCompanyName); });
                                c.Item().Text(t => { t.Span("Address line 1: ").FontColor(Copper); t.Span(details.ShipperAddress1); });
                                c.Item().Text(t => { t.Span("Address line 2: ").FontColor(Copper); t.Span(details.ShipperAddress2); });
                                c.Item().Text(t => { t.Span("Postcode / City: ").FontColor(Copper); t.Span(details.ShipperAddress3); });
                                c.Item().Text(t => { t.Span("Location: ").FontColor(Copper); t.Span(""); });
                                c.Item().Text(t => { t.Span("Sender name: ").FontColor(Copper); t.Span(""); });
                                c.Item().Text(t => { t.Span("Telephone / Email: ").FontColor(Copper); t.Span(""); });
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text(t => { t.Span("Invoice number (optional): ").FontColor(Copper); t.Span(header.InvoiceNumber); });
                                c.Item().Text(t => { t.Span("Shipping date: ").FontColor(Copper); t.Span(header.ShipDate?.ToString("dd/MM/yyyy") ?? ""); });
                                c.Item().Text(t => { t.Span("Shipment number: ").FontColor(Copper); t.Span(""); });
                                c.Item().Text(t => { t.Span("Currency: ").FontColor(Copper); t.Span(header.CurrencyCode ?? ""); });
                                c.Item().Text(t => { t.Span("Reason for export: ").FontColor(Copper); t.Span(""); });
                                c.Item().Text(t => { t.Span("Sender VAT number: ").FontColor(Copper); t.Span(""); });
                                c.Item().Text(t => { t.Span("Receiver VAT number: ").FontColor(Copper); t.Span(""); });
                                c.Item().Text(t => { t.Span("Terms of sale (Incoterms): ").FontColor(Copper); t.Span(header.TradeTermCode ?? ""); });
                            });
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Receiver details").Bold().FontColor(Copper);
                                c.Item().Text(t => { t.Span("Company: ").FontColor(Copper); t.Span(details.ConsigneeName); });
                                foreach (var line in details.ConsigneeAddressLines) c.Item().Text(line);
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Delivery details (if different from receiver)").Bold().FontColor(Copper).FontSize(8);
                                c.Item().Text(t => { t.Span("Company: ").FontColor(Copper); t.Span(details.NotifyPartyName); });
                                foreach (var line in details.NotifyPartyAddressLines) c.Item().Text(line);
                            });
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Description of goods
                                columns.RelativeColumn(1); // Quantity
                                columns.RelativeColumn(1); // Unit weight
                                columns.RelativeColumn(1); // Unit value
                                columns.RelativeColumn(1); // HS code
                                columns.RelativeColumn(1); // Location of origin
                                columns.RelativeColumn(1); // Total weight
                                columns.RelativeColumn(1); // Total value
                            });
                            table.Header(h =>
                            {
                                foreach (var label in new[] { "Description of goods", "Quantity", "Unit weight (kg)", "Unit value", "HS code", "Location of origin", "Total weight (kg)", "Total value" })
                                    h.Cell().Text(label).Bold().FontColor(Copper).FontSize(8);
                                h.Cell().ColumnSpan(8).PaddingTop(2).LineHorizontal(1).LineColor(Copper);
                            });
                            // One row per shipment line - Quantity comes from real
                            // per-line data; Unit weight, Unit value, HS code,
                            // Location of origin, Total weight and Total value
                            // have no field yet (see
                            // project_commercial_invoice_print_extra_fields_todo).
                            foreach (var line in details.Lines)
                            {
                                table.Cell().Text($"{line.Order} {line.StyleCode} {line.NewOrder}").FontSize(7);
                                table.Cell().Text(line.Quantity.ToString("N0")).FontSize(7);
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text("");
                                table.Cell().Text("");
                            }
                        });

                        if (!string.IsNullOrWhiteSpace(header.Detail))
                        {
                            column.Item().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                c.Item().Text("Description of goods (free text)").Bold().FontColor(Copper).FontSize(8);
                                c.Item().Text(header.Detail).FontFamily(Fonts.Consolas).FontSize(7);
                            });
                        }

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Text(t =>
                            {
                                t.Span("Number of packages in shipment: ").FontColor(Copper);
                                t.Span("");
                            });
                            row.RelativeItem().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                            {
                                foreach (var label in new[] { "Total shipment value", "Discount", "Subtotal", "Shipping costs", "Insurance costs", "Other costs", "Total declared value" })
                                    c.Item().Text(label).FontColor(Copper).FontSize(8);
                            });
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                        {
                            c.Item().Text("Declaration").Bold().FontColor(Copper);
                            c.Item().Text("I declare that the content of this invoice is true and correct.").FontSize(8);
                            c.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text("Name and Signature").FontSize(7).FontColor(TextMuted);
                                row.RelativeItem().Text("Company and Job title").FontSize(7).FontColor(TextMuted);
                                row.RelativeItem().Text("Date").FontSize(7).FontColor(TextMuted);
                            });
                        });

                        column.Item().Border(1).BorderColor(Copper).Padding(6).Column(c =>
                        {
                            c.Item().Text("Additional information (e.g. hazardous details, EORI number, ECCN number, etc.)").Bold().FontColor(Copper).FontSize(8);
                            c.Item().Text(JoinRemarks(header));
                            if (printAssessmentNo)
                                c.Item().Text(t => { t.Span("Ass. No.: ").FontColor(Copper); t.Span(header.AssessmentNumber ?? ""); });
                        });
                    });

                    PageNumberFooter(page);
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
