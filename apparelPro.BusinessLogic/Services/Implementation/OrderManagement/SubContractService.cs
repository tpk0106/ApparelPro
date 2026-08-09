using apparelPro.BusinessLogic.Services.Models.OrderManagement.ISubContractService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.SubContracting;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class SubContractService : ISubContractService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public SubContractService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<List<SubContractServiceModel>> GetSubContractsAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            string orderClean = order.Trim();
            string styleClean = styleCode.Trim();

            var rows = await _apparelProDbContext.SubContracts
                .AsNoTracking()
                .Where(sc => sc.BuyerCode == buyerCode && sc.Order == orderClean && sc.TypeCode == typeCode && sc.StyleCode == styleClean)
                .ToListAsync();

            if (rows.Count == 0)
            {
                return new List<SubContractServiceModel>();
            }

            var subContractorCodes = rows.Select(r => r.SubContractorCode).Distinct().ToList();
            var subContractorNames = await _apparelProDbContext.SubContractors
                .Where(s => subContractorCodes.Contains(s.Code))
                .ToDictionaryAsync(s => s.Code, s => s.Name);

            return rows.Select(row => new SubContractServiceModel
            {
                BuyerCode = row.BuyerCode,
                Order = row.Order,
                TypeCode = row.TypeCode,
                StyleCode = row.StyleCode,
                SubContractorCode = row.SubContractorCode,
                SubContractorName = subContractorNames.TryGetValue(row.SubContractorCode, out var name) ? name : row.SubContractorCode,
                SubQuantity = row.SubQuantity,
                CostPerGarment = row.CostPerGarment,
                Currency = row.Currency,
                Unit = row.Unit,
                ReceivedQuantity = row.ReceivedQuantity,
                BalanceQuantity = row.SubQuantity - row.ReceivedQuantity
            }).ToList();
        }

        // Non-blocking advisory check (per explicit 2026-08-09 user decision) - sums
        // SubQuantity across every Sub Contract row for the style, AFTER applying this
        // save, and compares it to the style's ordered quantity (References.Style.Quantity).
        // Returns null when there's nothing to warn about (no evidence this rule was ever
        // hard-enforced in the legacy source, so it never blocks - see class-level comment
        // on SubContract.cs).
        private async Task<string?> GetQuantityWarningAsync(
            int buyerCode, string order, int typeCode, string styleCode, string excludeSubContractorCode, decimal incomingSubQuantity)
        {
            var style = await _apparelProDbContext.Styles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode);

            decimal? orderedQuantity = style?.Quantity;
            if (orderedQuantity == null || orderedQuantity <= 0)
            {
                return null; // nothing to compare against
            }

            decimal otherRowsTotal = await _apparelProDbContext.SubContracts
                .AsNoTracking()
                .Where(sc => sc.BuyerCode == buyerCode && sc.Order == order && sc.TypeCode == typeCode &&
                             sc.StyleCode == styleCode && sc.SubContractorCode != excludeSubContractorCode)
                .SumAsync(sc => (decimal?)sc.SubQuantity) ?? 0;

            decimal projectedTotal = otherRowsTotal + incomingSubQuantity;
            if (projectedTotal > orderedQuantity)
            {
                return $"Total Sub Contract quantity for this style ({projectedTotal:N0}) now exceeds the ordered quantity ({orderedQuantity:N0}) by {projectedTotal - orderedQuantity:N0}.";
            }

            return null;
        }

        public async Task<SaveSubContractResultServiceModel> SaveSubContractAsync(SaveSubContractServiceModel request)
        {
            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string order = request.Order.Trim();
                string styleCode = request.StyleCode.Trim();
                string subContractorCode = request.SubContractorCode.Trim();
                string currency = request.Currency.Trim();
                string unit = request.Unit.Trim();

                // --- Validation (Zero-Assumption boundary: every FK must exist) ---
                var styleExists = await _apparelProDbContext.Styles
                    .AnyAsync(s => s.BuyerCode == request.BuyerCode && s.Order == order && s.TypeCode == request.TypeCode && s.StyleCode == styleCode);
                if (!styleExists)
                {
                    throw new InvalidOperationException("Invalid Buyer/Order/Type/Style.");
                }

                var subContractorExists = await _apparelProDbContext.SubContractors.AnyAsync(s => s.Code == subContractorCode);
                if (!subContractorExists)
                {
                    throw new InvalidOperationException("Invalid Sub Contractor Code.");
                }

                var currencyExists = await _apparelProDbContext.Currencies.AnyAsync(c => c.Code == currency);
                if (!currencyExists)
                {
                    throw new InvalidOperationException("Invalid Currency Code.");
                }

                var unitExists = await _apparelProDbContext.Units.AnyAsync(u => u.Code == unit);
                if (!unitExists)
                {
                    throw new InvalidOperationException("Invalid Unit Code.");
                }

                if (request.SubQuantity <= 0)
                {
                    throw new InvalidOperationException("Sub Contract Quantity must be greater than zero.");
                }
                if (request.CostPerGarment <= 0)
                {
                    throw new InvalidOperationException("Cost per Garment must be greater than zero.");
                }
                if (request.ReceivedQuantity < 0)
                {
                    throw new InvalidOperationException("Received Quantity cannot be negative.");
                }
                // Basic data-integrity guard (not a legacy-derived business rule) - a received
                // count can never physically exceed what was assigned to the sub-contractor.
                if (request.ReceivedQuantity > request.SubQuantity)
                {
                    throw new InvalidOperationException("Received Quantity cannot exceed Sub Contract Quantity.");
                }

                // --- Non-blocking advisory (per user decision) ---
                string? quantityWarning = await GetQuantityWarningAsync(
                    request.BuyerCode, order, request.TypeCode, styleCode, subContractorCode, request.SubQuantity);

                // --- Upsert ---
                var existing = await _apparelProDbContext.SubContracts
                    .FirstOrDefaultAsync(sc => sc.BuyerCode == request.BuyerCode && sc.Order == order && sc.TypeCode == request.TypeCode &&
                                                sc.StyleCode == styleCode && sc.SubContractorCode == subContractorCode);

                if (existing != null)
                {
                    existing.SubQuantity = request.SubQuantity;
                    existing.CostPerGarment = request.CostPerGarment;
                    existing.Currency = currency;
                    existing.Unit = unit;
                    existing.ReceivedQuantity = request.ReceivedQuantity;
                    _apparelProDbContext.SubContracts.Update(existing);
                }
                else
                {
                    _apparelProDbContext.SubContracts.Add(new SubContract
                    {
                        BuyerCode = request.BuyerCode,
                        Order = order,
                        TypeCode = request.TypeCode,
                        StyleCode = styleCode,
                        SubContractorCode = subContractorCode,
                        SubQuantity = request.SubQuantity,
                        CostPerGarment = request.CostPerGarment,
                        Currency = currency,
                        Unit = unit,
                        ReceivedQuantity = request.ReceivedQuantity
                    });
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                var subContractorName = (await _apparelProDbContext.SubContractors.FirstOrDefaultAsync(s => s.Code == subContractorCode))?.Name ?? subContractorCode;

                return new SaveSubContractResultServiceModel
                {
                    SubContract = new SubContractServiceModel
                    {
                        BuyerCode = request.BuyerCode,
                        Order = order,
                        TypeCode = request.TypeCode,
                        StyleCode = styleCode,
                        SubContractorCode = subContractorCode,
                        SubContractorName = subContractorName,
                        SubQuantity = request.SubQuantity,
                        CostPerGarment = request.CostPerGarment,
                        Currency = currency,
                        Unit = unit,
                        ReceivedQuantity = request.ReceivedQuantity,
                        BalanceQuantity = request.SubQuantity - request.ReceivedQuantity
                    },
                    QuantityWarning = quantityWarning
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteSubContractAsync(int buyerCode, string order, int typeCode, string styleCode, string subContractorCode)
        {
            string orderClean = order.Trim();
            string styleClean = styleCode.Trim();
            string subContractorClean = subContractorCode.Trim();

            var existing = await _apparelProDbContext.SubContracts
                .FirstOrDefaultAsync(sc => sc.BuyerCode == buyerCode && sc.Order == orderClean && sc.TypeCode == typeCode &&
                                            sc.StyleCode == styleClean && sc.SubContractorCode == subContractorClean);
            if (existing == null)
            {
                return false;
            }

            _apparelProDbContext.SubContracts.Remove(existing);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
