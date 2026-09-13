using apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Redraws the real Sri Lanka Customs Form 308A "Value Declaration" (VDF)
    // the user supplied - a fully re-themed digital document (dark/copper),
    // laid out from the real form's own numbered-box structure (drawn as
    // real bordered rectangles, matching the paper form) rather than an
    // overlay onto physical stationery, same approach as the other IE print
    // engines with no supplied pre-printed image asset.
    //
    // Serves both flows that share ValueDeclarationHeader (standalone and
    // invoice-scoped, see the entity's own comment): Exporter/Importer come
    // from `details.Exporter*`/`details.Importer*` when the invoice-scoped
    // flow resolved them, falling back to the header's own plain
    // ExporterName/ExporterAddress and standalone Invoice No./Date otherwise.
    public static class ValueDeclarationPrintEngine
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
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(8.5f).FontColor(TextWhite));
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

        // Real rectangle box (matching the paper form's own box layout),
        // not just plain text - addresses "print rectangle boxes as it is".
        private static IContainer Box(IContainer container) =>
            container.Border(1).BorderColor(Copper).Padding(6);

        private static void BoxHeader(ColumnDescriptor column, string label) =>
            column.Item().Text(label).FontColor(TextMuted).FontSize(7);

        private static void YesNo(ColumnDescriptor column, string label, bool? value)
        {
            Box(column.Item()).Column(c => c.Item().Text(t =>
            {
                t.Span($"{label} ").FontColor(TextMuted).FontSize(8);
                t.Span(value == null ? "-" : (value.Value ? "Yes" : "No")).Bold();
            }));
        }

        public static byte[] GeneratePdf(ValueDeclarationPrintDetailsServiceModel details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var header = details.Header;

            var exporterName = details.ExporterCompanyName ?? header.ExporterName;
            var exporterAddressLines = !string.IsNullOrWhiteSpace(details.ExporterAddress1)
                ? new[] { details.ExporterAddress1, details.ExporterAddress2, details.ExporterCityPostCodeCountry }
                : new[] { header.ExporterAddress };
            var invoiceNumberDisplay = header.InvoiceNumber ?? header.InvoiceNo;
            var invoiceDateDisplay = details.InvoiceDate ?? header.InvoiceDate?.ToDateTime(TimeOnly.MinValue);

            using var memoryStream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    PageNumberFooter(page);
                    page.Content().Column(column =>
                    {
                        // Matches the real form exactly: a tall "CusDec No."
                        // box sits on the right, spanning the combined height
                        // of the title strip (with Year/Office Code/S-L as
                        // short boxes at its top) and boxes 1/2 below it -
                        // not stacked, side by side, per the user's supplied
                        // screenshot of the real form's top section.
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3.3f).Column(left =>
                            {
                                left.Item().Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("CUSTOMS 308A").Bold().FontSize(9);
                                        c.Item().PaddingTop(3).Text(t =>
                                        {
                                            t.Span("SRI LANKA CUSTOMS ").Bold().FontSize(15).FontColor(Copper);
                                            t.Span("VALUE DECLARATION").FontSize(13).FontColor(Copper);
                                        });
                                        c.Item().PaddingTop(3)
                                            .Text("(This Declaration shall not be required for goods imported as samples of no commercial value)")
                                            .Italic().FontSize(7.5f).FontColor(TextMuted);
                                    });
                                    r.ConstantItem(65).Column(c =>
                                    {
                                        c.Item().Text("Year").FontSize(8);
                                        c.Item().PaddingTop(2).Element(e => Box(e).Text(header.Year ?? ""));
                                    });
                                    r.ConstantItem(85).Column(c =>
                                    {
                                        c.Item().Text("Office Code:").FontSize(8);
                                        c.Item().PaddingTop(2).Element(e => Box(e).Text(header.OfficeCode ?? ""));
                                    });
                                    r.ConstantItem(50).Column(c =>
                                    {
                                        c.Item().Text("S/L:").FontSize(8);
                                        c.Item().PaddingTop(2).Element(e => Box(e).Text(header.SeriesLetter ?? ""));
                                    });
                                });
                                left.Item().PaddingTop(8).Row(r2 =>
                                {
                                    r2.RelativeItem().Element(e => Box(e).Column(c =>
                                    {
                                        BoxHeader(c, "1. Exporter's Name & Address");
                                        c.Item().Text(exporterName).Bold();
                                        foreach (var line in exporterAddressLines)
                                            if (!string.IsNullOrWhiteSpace(line))
                                                c.Item().Text(line).FontSize(8);
                                    }));
                                    r2.RelativeItem().Element(e => Box(e).Column(c =>
                                    {
                                        BoxHeader(c, "2. Indenting Agent's Name & Address");
                                        c.Item().Text(header.IndentingAgentName ?? "");
                                        if (!string.IsNullOrWhiteSpace(header.IndentingAgentAddress))
                                            c.Item().Text(header.IndentingAgentAddress).FontSize(8);
                                    }));
                                });
                            });
                            row.ConstantItem(8);
                            row.ConstantItem(130).Column(c =>
                            {
                                c.Item().Text("CusDec No.").FontSize(8);
                                c.Item().PaddingTop(2).Element(e => Box(e).MinHeight(90).Text(header.CusDecNo ?? ""));
                            });
                        });

                        column.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Copper);
                        column.Item().PaddingTop(6);

                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "3. Importer VAT No.");
                                c.Item().Text(header.ImporterVatNo ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "4. Declarant VAT No.");
                                c.Item().Text(header.DeclarantVatNo ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "5. Sales Contract No. / Date");
                                c.Item().Text($"{header.SalesContractNo}  {header.SalesContractDate:dd/MM/yyyy}");
                            }));
                        });

                        // Real form runs box 11's cost breakdown as a right-hand
                        // column alongside boxes 6-10/12-15 on the left, not
                        // stacked below them - matches the paper form's layout.
                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem(3).Column(left =>
                            {
                                left.Item().Row(r =>
                                {
                                    r.RelativeItem().Element(e => Box(e).Column(c =>
                                    {
                                        BoxHeader(c, "6. Invoice No. / Date");
                                        c.Item().Text($"{invoiceNumberDisplay}  {invoiceDateDisplay:dd/MM/yyyy}");
                                    }));
                                    r.RelativeItem().Element(e => Box(e).Column(c =>
                                    {
                                        BoxHeader(c, "7. Total Invoice Value");
                                        c.Item().Text($"{details.CurrencyDescription} {header.TotalInvoiceValue:N2}");
                                    }));
                                });
                                left.Item().PaddingTop(4).Element(e => Box(e).Column(c =>
                                {
                                    BoxHeader(c, "8. Nature of Transaction");
                                    c.Item().Text(header.NatureOfTransaction ?? "");
                                }));
                                left.Item().PaddingTop(4).Row(r =>
                                {
                                    r.RelativeItem().Element(e => Box(e).Column(c =>
                                    {
                                        BoxHeader(c, "9. Currency of Payment");
                                        c.Item().Text(details.CurrencyDescription);
                                    }));
                                    r.RelativeItem().Element(e => Box(e).Column(c =>
                                    {
                                        BoxHeader(c, "10. Terms of Delivery");
                                        c.Item().Text(details.TermsOfDeliveryDescription);
                                    }));
                                });
                                left.Item().PaddingTop(4);
                                YesNo(left, "12 Are you related to the seller in terms of Article 9 of Schedule E of the Customs Ordinance :", header.IsRelatedToSeller);
                                YesNo(left, "13 If related, was the value influenced by the relationship ?", header.WasValueInfluencedByRelationship);
                                YesNo(left, "14 Is the sale subject any conditions or restriction imposed by the seller ?", header.IsSaleSubjectToConditions);
                                left.Item().PaddingTop(2).Element(e => Box(e).Column(c =>
                                {
                                    c.Item().Text(t =>
                                    {
                                        t.Span("15 Previous imports of identical/similar goods, if any (within last three months) ").FontColor(TextMuted).FontSize(8);
                                        t.Span(header.HasPreviousImportsLast3Months == null ? "-" : (header.HasPreviousImportsLast3Months.Value ? "Yes" : "No")).Bold();
                                    });
                                    if (!string.IsNullOrWhiteSpace(header.PreviousImportsDetails))
                                        c.Item().Text(header.PreviousImportsDetails).FontSize(8);
                                }));
                            });

                            row.ConstantItem(10);

                            row.RelativeItem(2).Element(e => Box(e).Column(c =>
                            {
                                c.Item()
                                    .Text("11 Declare any of the following costs & services not included in the invoice value in terms of article 8 (1) & 8 (2) of Schedule E of the Customs Ordinance")
                                    .FontColor(Copper).FontSize(8).Bold();
                                c.Item().PaddingTop(4).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(3);
                                        cols.RelativeColumn(1);
                                    });
                                    void Cost(string label, decimal value)
                                    {
                                        table.Cell().Padding(2).Text(label).FontSize(7.5f).FontColor(TextMuted);
                                        table.Cell().Padding(2).AlignRight().Text(value.ToString("N2"));
                                    }
                                    Cost("(a) Brokerage and Commissions", header.BrokerageCommission);
                                    Cost("(b) Cost of Containers", header.CostOfContainers);
                                    Cost("(c) Packing costs", header.PackingCosts);
                                    Cost("(d) Cost of goods/services supplied by buyer", header.CostOfGoodsSuppliedByBuyer);
                                    Cost("(e) Royalties and License Fees", header.RoyaltiesLicenseFees);
                                    Cost("(f) Value of proceeds accrued to seller", header.ProceedsToSeller);
                                    Cost("(g) Loading, Unloading, Handling charges", header.LoadingHandlingCharges);
                                    Cost("(h) Insurance", header.Insurance);
                                    Cost("(i) Freight", header.Freight);
                                    Cost("(j) Other payments, if any", header.OtherPayments);
                                });
                            }));
                        });

                        column.Item().PaddingTop(10).Text("16. Complete Description of Goods").FontColor(Copper).FontSize(9.5f).Bold();
                        if (details.Lines.Count == 0)
                        {
                            column.Item().Text("No goods lines entered.").FontColor(TextMuted);
                        }
                        else
                        {
                            column.Item().Border(1).BorderColor(Copper).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(28);
                                    c.RelativeColumn(2.4f);
                                    c.RelativeColumn(1.1f);
                                    c.RelativeColumn(1.1f);
                                    c.RelativeColumn(0.8f);
                                    c.RelativeColumn(0.6f);
                                    c.RelativeColumn(0.6f);
                                    c.RelativeColumn(0.9f);
                                    c.RelativeColumn(1f);
                                    c.RelativeColumn(1f);
                                    c.RelativeColumn(0.9f);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Padding(3).Text("Item").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("Description").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("Brand").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("Model").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("Size").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("CO").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("UOM").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).AlignRight().Text("Quantity").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).AlignRight().Text("Value").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).Text("HS Code").FontColor(TextMuted).FontSize(7);
                                    h.Cell().Padding(3).AlignRight().Text("Weight").FontColor(TextMuted).FontSize(7);
                                });
                                foreach (var line in details.Lines)
                                {
                                    table.Cell().Padding(3).Text(line.ItemNo.ToString());
                                    table.Cell().Padding(3).Text(line.Description);
                                    table.Cell().Padding(3).Text(line.Brand ?? "");
                                    table.Cell().Padding(3).Text(line.Model ?? "");
                                    table.Cell().Padding(3).Text(line.Size ?? "");
                                    table.Cell().Padding(3).Text(line.CountryOfOriginDescription);
                                    table.Cell().Padding(3).Text(line.UnitDescription);
                                    table.Cell().Padding(3).AlignRight().Text(line.Quantity.ToString("N2"));
                                    table.Cell().Padding(3).AlignRight().Text(line.Value.ToString("N2"));
                                    table.Cell().Padding(3).Text(line.HsCode ?? "");
                                    table.Cell().Padding(3).AlignRight().Text(line.Weight?.ToString("N2") ?? "-");
                                }
                            });
                        }
                        column.Item().PaddingTop(2)
                            .Text("(S/L) Series Letter, (CO) Country of Origin, (UOM) Unit of Measure")
                            .FontSize(7).FontColor(TextMuted);

                        column.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "17. Terms of Payment");
                                c.Item().Text(details.TermsOfPaymentDescription);
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "18. Port of Shipment");
                                c.Item().Text(details.PortOfShipmentDescription);
                            }));
                        });
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "19. AWB/BL No. / Date");
                                c.Item().Text($"{header.AwbBlNo}  {header.AwbBlDate:dd/MM/yyyy}");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "20. Importer's Name and Address");
                                c.Item().Text(details.ImporterCompanyName).Bold();
                                c.Item().Text(details.ImporterAddress1).FontSize(8);
                                c.Item().Text(details.ImporterAddress2).FontSize(8);
                                c.Item().Text(details.ImporterCityPostCodeCountry).FontSize(8);
                            }));
                        });

                        column.Item().PaddingTop(10).Text("DECLARATION").FontColor(Copper).FontSize(9).Bold();
                        column.Item().Text("1. I hereby declare that the information furnished in this form and in the continuation sheets is true and correct in every respect.").FontSize(8);
                        column.Item().PaddingTop(2).Text("2. I also undertake to bring to the notice of Customs any particulars, which may subsequently come to my knowledge, that will have a bearing on valuation.").FontSize(8);
                        column.Item().PaddingTop(2).Text(t =>
                        {
                            t.Span("3. No of continuation sheets, if any: ").FontSize(8);
                            t.Span(header.ContinuationSheetsCount?.ToString() ?? "0").Bold();
                        });
                        column.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Name of signatory: ").FontColor(TextMuted).FontSize(8);
                                t.Span(header.SignatoryName ?? "");
                            });
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Signature of importer/Agent: ").FontColor(TextMuted).FontSize(8);
                            });
                        });
                        column.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Title: ").FontColor(TextMuted).FontSize(8);
                                t.Span(header.SignatoryTitle ?? "");
                            });
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Date: ").FontColor(TextMuted).FontSize(8);
                                t.Span(header.SignatoryDate?.ToString("dd/MM/yyyy") ?? "");
                            });
                        });
                        column.Item().PaddingTop(2).Text(t =>
                        {
                            t.Span("Name of company: ").FontColor(TextMuted).FontSize(8);
                            t.Span(header.SignatoryCompanyName ?? "");
                        });

                        column.Item().PaddingTop(10).Text("FOR OFFICE USE").FontColor(Copper).FontSize(9).Bold();
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "Appraiser's comments");
                                c.Item().Text(header.AppraiserComments ?? "Satisfied / Doubt / suspect fraud").FontSize(8);
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "SC's comments");
                                c.Item().Text(header.ScComments ?? "Satisfied / Doubt / suspect fraud").FontSize(8);
                            }));
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Valuation Reference No.: ").FontColor(TextMuted).FontSize(8);
                                t.Span(header.ValuationReferenceNo ?? "");
                            });
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Central Valuation Division: ").FontColor(TextMuted).FontSize(8);
                                t.Span(header.CentralValuationEndorsement ?? "");
                            });
                        });
                    });
                });
            }).GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
