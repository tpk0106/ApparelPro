using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorQuantityRatioService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class ColorQuantityRatioService : IColorQuantityRatioService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public ColorQuantityRatioService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<List<ColorQuantityRatioServiceModel>> GetByStyleAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var records = await _apparelProDbContext.ColorQuantityRatios
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode &&
                            d.Order == order &&
                            d.TypeCode == typeCode &&
                            d.StyleCode == styleCode)
                .ToListAsync();

            return _mapper.Map<List<ColorQuantityRatioServiceModel>>(records);
        }

        // SUPPLIER PO LOCK GUARD: once a Supplier Purchase Order has been raised against this
        // exact Buyer/Order/Type/Style, the colour/size allocation that fed its material
        // consumption calculation is frozen - absolute, no override, matching the same real-world
        // reasoning as Style.Quantity's own Supplier PO lock (StyleDetailsService
        // .ValidateStyleQuantityAsync). Scoped to the exact style (not the whole order) since a
        // Supplier PO is raised per style - locking every style in the order would be too broad.
        private async Task EnsureNotLockedBySupplierPoAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var hasSupplierPurchaseOrder = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .AsNoTracking()
                .AnyAsync(d => d.Buyer == buyerCode && d.Order == order && d.Type == typeCode && d.Style == styleCode);

            if (hasSupplierPurchaseOrder)
                throw new InvalidOperationException(
                    $"Colour/Size Breakdown for Style '{styleCode}' can no longer be changed - " +
                    "a Supplier Purchase Order has already been raised against this style.");
        }

        public async Task<bool> BulkSaveColorQuantityRatiosAsync(int buyerCode, string order, int typeCode, string styleCode,
            List<CreateColorQuantityRatioServiceModel> records)
        {
            if (records == null || !records.Any())
                throw new InvalidOperationException("At least one colour allocation row is required.");

            await EnsureNotLockedBySupplierPoAsync(buyerCode, order, typeCode, styleCode);

            var style = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode)
                .FirstOrDefaultAsync();

            if (style == null)
                throw new InvalidOperationException("Style not found for the given Buyer/Order/Type/Style.");

            var target = (int)Math.Round(style.Quantity ?? 0, MidpointRounding.AwayFromZero);
            var isRatioMode = string.Equals(style.ColorRatio?.Trim(), "R", StringComparison.OrdinalIgnoreCase);

            // Server is authoritative for the actual Quantity per colour - the
            // client's own computed number is never trusted, only its Ratio
            // (Ratio mode) or its Quantity (Quantity mode, still validated).
            if (isRatioMode)
            {
                var weights = records.Select(r => r.Ratio).ToList();
                var allocated = RatioAllocator.Allocate(target, weights);
                for (var i = 0; i < records.Count; i++)
                {
                    records[i].Quantity = allocated[i];
                }
            }
            else
            {
                var postedTotal = records.Sum(r => r.Quantity);
                if (postedTotal != target)
                    throw new InvalidOperationException(
                        $"Colour allocation does not reconcile: entered total is {postedTotal:N2} Pcs but the Style's target quantity is {target:N2} Pcs.");
                foreach (var r in records)
                {
                    r.Ratio = 0;
                }
            }

            var strategy = _apparelProDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
                try
                {
                    var existingRecords = await _apparelProDbContext.ColorQuantityRatios
                        .Where(d => d.BuyerCode == buyerCode &&
                                    d.Order == order &&
                                    d.TypeCode == typeCode &&
                                    d.StyleCode == styleCode)
                        .ToListAsync();

                    if (existingRecords.Any())
                    {
                        _apparelProDbContext.ColorQuantityRatios.RemoveRange(existingRecords);
                    }

                    var dbModels = _mapper.Map<List<ColorQuantityRatio>>(records);
                    await _apparelProDbContext.ColorQuantityRatios.AddRangeAsync(dbModels);

                    await _apparelProDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }

        public async Task SetColorRatioModeAsync(int buyerCode, string order, int typeCode, string styleCode, string mode)
        {
            await SetRatioModeAsync(buyerCode, order, typeCode, styleCode, mode, isColorMode: true);
        }

        public async Task SetSizeRatioModeAsync(int buyerCode, string order, int typeCode, string styleCode, string mode)
        {
            await SetRatioModeAsync(buyerCode, order, typeCode, styleCode, mode, isColorMode: false);
        }

        private async Task SetRatioModeAsync(int buyerCode, string order, int typeCode, string styleCode, string mode, bool isColorMode)
        {
            await EnsureNotLockedBySupplierPoAsync(buyerCode, order, typeCode, styleCode);

            var normalizedMode = mode.Trim().ToUpper() == "R" ? "R" : "Q";

            var styleDbModel = await _apparelProDbContext.Styles
                .Where(s => s.BuyerCode == buyerCode &&
                            s.Order == order &&
                            s.TypeCode == typeCode &&
                            s.StyleCode == styleCode)
                .FirstOrDefaultAsync();

            if (styleDbModel == null)
                throw new InvalidOperationException("Style not found for the given Buyer/Order/Type/Style.");

            if (isColorMode)
                styleDbModel.ColorRatio = normalizedMode;
            else
                styleDbModel.SizeRatio = normalizedMode;

            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
