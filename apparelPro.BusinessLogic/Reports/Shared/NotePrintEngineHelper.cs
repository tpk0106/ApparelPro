using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace apparelPro.BusinessLogic.Reports.Shared
{
    // Shared QuestPDF scaffolding for every note print engine (General Inventory and
    // Orderwise Inventory alike) - every one of them repeats the same page setup,
    // repeating page header (title + printed Date/Time, bold "<Label> No : <Number>"
    // line, a divider, note-specific detail line(s), a closing divider), a
    // Prepared/Checked/Authorised By signature block printed once at the true end of
    // the document (not per page - QuestPDF's page.Content() is a single flowing
    // column across pages, so this naturally reproduces legacy's single "do inv_foot"
    // call), and a page-number footer. Only the title, document number, and the
    // note-specific detail row(s) actually differ between notes - everything else was
    // duplicated verbatim across every *PrintEngine.cs file before this extraction.
    public static class NotePrintEngineHelper
    {
        public static void ConfigureStandardPage(this PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));
        }

        // renderDetails builds whatever note-specific line(s) sit between the two
        // dividers (e.g. STRN's Buyer/Order/Department row, GRN's P/O+Supplier+Currency
        // row followed by an Invoice No row, DGN's single Stores line). Pass null for a
        // note with nothing to show there.
        public static void RenderNoteHeader(
            this PageDescriptor page,
            string title,
            string documentNumberLabel,
            string documentNumber,
            DateTime printedOn,
            Action<ColumnDescriptor>? renderDetails = null)
        {
            page.Header().Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(title).FontSize(14).Bold();
                    row.ConstantItem(160).Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text($"Date : {printedOn:dd/MM/yyyy}").FontSize(9);
                        dateCol.Item().AlignRight().Text($"Time : {printedOn:HH:mm}").FontSize(9);
                    });
                });
                column.Item().PaddingTop(2).Text($"{documentNumberLabel} : {documentNumber}").Bold();

                column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);

                if (renderDetails != null)
                    column.Item().PaddingTop(6).Column(renderDetails);

                column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
            });
        }

        public static void RenderSignatureBlock(
            this ColumnDescriptor column,
            string leftLabel = "Prepared By",
            string middleLabel = "Checked By",
            string rightLabel = "Authorised By")
        {
            column.Item().PaddingTop(40).Row(row =>
            {
                row.RelativeItem().Column(sig =>
                {
                    sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                    sig.Item().PaddingTop(2).Text(leftLabel).FontSize(9);
                });
                row.RelativeItem().Column(sig =>
                {
                    sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                    sig.Item().PaddingTop(2).Text(middleLabel).FontSize(9);
                });
                row.RelativeItem().Column(sig =>
                {
                    sig.Item().PaddingRight(20).LineHorizontal(1).LineColor(Colors.Black);
                    sig.Item().PaddingTop(2).Text(rightLabel).FontSize(9);
                });
            });
        }

        public static void RenderPageNumberFooter(this PageDescriptor page)
        {
            page.Footer().AlignCenter().Text(x =>
            {
                x.DefaultTextStyle(TextStyle.Default.FontSize(8));
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        }
    }
}
