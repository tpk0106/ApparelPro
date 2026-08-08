using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_CLSZ3.PRG's "COLOUR / SIZE DETAILS" print program - see
    // ColorSizeReportServiceModel's SCOPE NOTE for how legacy's od_clqr query is
    // derived here from ColorSizeDetails by grouping, not by guessing a new table.
    public class ColorSizeReportService : IColorSizeReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ColorSizeReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ColorSizeReportServiceModel> GetColorSizeReportAsync(int buyerCode, string order)
        {
            order = order.Trim();

            var buyerRow = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);

            var detailRows = await _apparelProDbContext.ColorSizeDetails
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order)
                .ToListAsync();

            if (detailRows.Count == 0)
                // Same wording as legacy's own error box for this exact condition
                // ("No Colour/Size Details For Printing").
                throw new InvalidOperationException("No Colour/Size Details For Printing.");

            // Fixed column set for the whole report - every Style block uses the same
            // Sizes, in the same order, even if a given Style/Colour has no row for one.
            var sizeColumns = detailRows
                .Select(d => d.Size)
                .Distinct()
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToList();

            var styles = detailRows
                .GroupBy(d => new { d.TypeCode, d.StyleCode })
                .OrderBy(g => g.Key.TypeCode)
                .ThenBy(g => g.Key.StyleCode)
                .Select(styleGroup =>
                {
                    var colours = styleGroup
                        .GroupBy(d => d.Color)
                        .OrderBy(g => g.Key, StringComparer.Ordinal)
                        .Select(colourGroup =>
                        {
                            var sizeQuantities = colourGroup
                                .GroupBy(d => d.Size)
                                .ToDictionary(g => g.Key, g => g.Sum(d => d.Qty));

                            // Every row for a given Colour was written with the same
                            // Description in one save (see ColorSizeDetails' own
                            // denormalization comment) - first non-blank value wins,
                            // covering rows saved before that field existed.
                            var description = colourGroup
                                .Select(d => d.Description)
                                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d)) ?? "";

                            return new ColorSizeReportColourServiceModel
                            {
                                ColorCode = colourGroup.Key,
                                Description = description,
                                SizeQuantities = sizeQuantities,
                                TotalQuantity = sizeQuantities.Values.Sum(),
                            };
                        })
                        .ToList();

                    var sizeTotals = sizeColumns.ToDictionary(
                        size => size,
                        size => colours.Sum(c => c.SizeQuantities.TryGetValue(size, out var qty) ? qty : 0));

                    return new ColorSizeReportStyleServiceModel
                    {
                        StyleCode = styleGroup.Key.StyleCode,
                        Colours = colours,
                        SizeTotals = sizeTotals,
                        GrandTotal = sizeTotals.Values.Sum(),
                    };
                })
                .ToList();

            return new ColorSizeReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerRow?.Name ?? "",
                Order = order,
                SizeColumns = sizeColumns,
                Styles = styles,
            };
        }
    }
}
