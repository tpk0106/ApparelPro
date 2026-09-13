using apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.ImportExport
{
    // Redraws the real Sri Lanka "Application for an Export Control License"
    // form (Department of Imports & Exports Control) the user supplied - a
    // fully re-themed digital document (dark/copper), laid out from the
    // real form's own box structure. Not a migration of legacy
    // ie_elic2.prg, which is a quota-based print artifact for the old MFA
    // quota system (industry-dead) - a different document entirely.
    public static class ExportLicensePrintEngine
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

        private static IContainer Box(IContainer container) =>
            container.Border(1).BorderColor(Copper).Padding(6);

        private static void BoxHeader(ColumnDescriptor column, string label) =>
            column.Item().Text(label).FontColor(TextMuted).FontSize(7);

        public static byte[] GeneratePdf(ExportLicensePrintDetailsServiceModel details)
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
                        column.Item().Text("Import and Export Control Department").FontColor(TextMuted).FontSize(8);
                        column.Item().PaddingBottom(6)
                            .Text("APPLICATION FOR AN EXPORT CONTROL LICENSE").Bold().FontSize(13).FontColor(Copper);
                        column.Item().LineHorizontal(0.5f).LineColor(Copper);
                        column.Item().PaddingTop(6);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "01. Type of Applicant");
                                c.Item().Text(header.ApplicantType ?? "");
                            }));
                            row.RelativeItem(2).Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "Applicant ID (Office Use Only)");
                                c.Item().Text(header.ApplicantIdOfficeUse ?? "");
                            }));
                        });

                        column.Item().PaddingTop(4).Element(e => Box(e).Column(c =>
                        {
                            BoxHeader(c, "02. Company Name (If Individual, Name in full)");
                            c.Item().Text(details.ApplicantCompanyName).Bold();
                            if (!string.IsNullOrWhiteSpace(details.ApplicantAddress1)) c.Item().Text(details.ApplicantAddress1).FontSize(8);
                            if (!string.IsNullOrWhiteSpace(details.ApplicantAddress2)) c.Item().Text(details.ApplicantAddress2).FontSize(8);
                            if (!string.IsNullOrWhiteSpace(details.ApplicantCityPostCodeCountry)) c.Item().Text(details.ApplicantCityPostCodeCountry).FontSize(8);
                        }));

                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "03. Business Registration No. (If Individual, NIC or Passport No.)");
                                c.Item().Text(header.BusinessRegistrationNo ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "04. VAT Registration No.");
                                c.Item().Text(header.VatRegistrationNo ?? "");
                            }));
                        });

                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "06. Telephone");
                                c.Item().Text(header.Telephone ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "Fax");
                                c.Item().Text(header.Fax ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "07. E-mail");
                                c.Item().Text(header.Email ?? "");
                            }));
                        });

                        column.Item().PaddingTop(10).Text("Basic Information").FontColor(Copper).FontSize(9.5f).Bold();
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "08. License Type");
                                c.Item().Text(header.LicenseType ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "09. Exchange Type");
                                c.Item().Text(header.ExchangeType ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "10. Commercial Type");
                                c.Item().Text(header.CommercialType ?? "");
                            }));
                        });
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "11. Name of the Bank");
                                c.Item().Text(details.BankName);
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "12. Mode of Payment");
                                c.Item().Text(header.ModeOfPayment ?? "");
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "13. Mode of Transportation");
                                c.Item().Text(header.ModeOfTransportation ?? "");
                            }));
                        });

                        column.Item().PaddingTop(10).Text("14. Movement of Goods - Name and Address of the Consignees")
                            .FontColor(Copper).FontSize(9.5f).Bold();
                        column.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "Consignee 1");
                                c.Item().Text(details.Consignee1Name).Bold();
                                if (!string.IsNullOrWhiteSpace(details.Consignee1Address))
                                    c.Item().Text(details.Consignee1Address).FontSize(8);
                            }));
                            row.RelativeItem().Element(e => Box(e).Column(c =>
                            {
                                BoxHeader(c, "Consignee 2");
                                c.Item().Text(details.Consignee2Name).Bold();
                                if (!string.IsNullOrWhiteSpace(details.Consignee2Address))
                                    c.Item().Text(details.Consignee2Address).FontSize(8);
                            }));
                        });

                        column.Item().PaddingTop(10).Text("15. HS Number / Description / Goods Detail").FontColor(Copper).FontSize(9.5f).Bold();
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
                                    c.ConstantColumn(18);
                                    c.RelativeColumn(0.9f);
                                    c.RelativeColumn(1.7f);
                                    c.RelativeColumn(0.55f);
                                    c.RelativeColumn(0.45f);
                                    c.RelativeColumn(0.95f);
                                    c.RelativeColumn(0.95f);
                                    c.RelativeColumn(1.15f);
                                    c.RelativeColumn(0.95f);
                                    c.RelativeColumn(0.95f);
                                    c.RelativeColumn(0.95f);
                                    c.RelativeColumn(1.05f);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Padding(2).Text("Item").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).Text("HS Number").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).Text("Description").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).Text("Pack Size").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).Text("UOM").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).AlignRight().Text("Quantity").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).AlignRight().Text("Unit Price").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).AlignRight().Text("Cost of Item (Unit Price x Qty)").FontColor(TextMuted).FontSize(6);
                                    h.Cell().Padding(2).AlignRight().Text("Insurance").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).AlignRight().Text("Freight").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).AlignRight().Text("Cost").FontColor(TextMuted).FontSize(6.5f);
                                    h.Cell().Padding(2).AlignRight().Text("Total CIF").FontColor(TextMuted).FontSize(6.5f);
                                });
                                foreach (var line in details.Lines)
                                {
                                    var costOfItem = line.UnitPrice * line.Quantity;
                                    var cost = costOfItem + line.Insurance + line.Freight;
                                    table.Cell().Padding(2).Text(line.ItemNo.ToString()).FontSize(7f);
                                    table.Cell().Padding(2).Text(line.HsNumber ?? "").FontSize(7f);
                                    table.Cell().Padding(2).Text(line.Description).FontSize(7f);
                                    table.Cell().Padding(2).Text(line.PackSize ?? "").FontSize(7f);
                                    table.Cell().Padding(2).Text(line.UnitCode ?? "").FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(line.Quantity.ToString("N2")).FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(line.UnitPrice.ToString("N2")).FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(costOfItem.ToString("N2")).FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(line.Insurance.ToString("N2")).FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(line.Freight.ToString("N2")).FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(cost.ToString("N2")).FontSize(7f);
                                    table.Cell().Padding(2).AlignRight().Text(line.TotalCif.ToString("N2")).FontSize(7f);
                                }
                            });
                        }

                        column.Item().PaddingTop(8).Element(e => Box(e).Column(c =>
                        {
                            BoxHeader(c, "16. Purpose of Exportation (If NFE Basis)");
                            c.Item().Text(header.PurposeOfExportation ?? "");
                        }));
                        column.Item().PaddingTop(4).Element(e => Box(e).Column(c =>
                        {
                            BoxHeader(c, "17. Use of the Commodity/Raw Material");
                            c.Item().Text(header.UseOfCommodity ?? "");
                        }));

                        column.Item().PaddingTop(10).Text("Declaration of the Applicant").FontColor(Copper).FontSize(9).Bold();
                        column.Item().Text("I apply for an export license in respect of goods described above and I declare that the particulars furnished by me are true & correct.").FontSize(8);
                        column.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                // Blank spacer matching the date value's own text
                                // height, so both underlines land on the same row -
                                // the signature line stays empty (leaving real room
                                // to sign), while the date is printed (known,
                                // not hand-written) sitting on its own line.
                                c.Item().Text(" ").FontSize(8);
                                c.Item().LineHorizontal(0.75f).LineColor(TextMuted);
                                c.Item().PaddingTop(2).Text("Signature").FontColor(TextMuted).FontSize(8);
                                c.Item().Text("(must be registered with the licensing Unit as an authorized signatory)").FontSize(7).FontColor(TextMuted);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem(1).Column(c =>
                            {
                                c.Item().AlignCenter().Text(header.SignatoryDate?.ToString("dd/MM/yyyy") ?? "").FontSize(8);
                                c.Item().LineHorizontal(0.75f).LineColor(TextMuted);
                                c.Item().AlignRight().PaddingTop(2).Text("Date").FontColor(TextMuted).FontSize(8);
                            });
                        });
                    });
                });
            }).GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
