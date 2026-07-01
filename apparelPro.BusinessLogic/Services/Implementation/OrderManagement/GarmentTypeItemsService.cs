using ApparelPro.Data;
using ApparelPro.WebApi.APIModels.OrderManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class GarmentTypeItemsService:IGarmentTypeItemsService
    {
        private readonly ApparelProDbContext  _apparelProDbContext;

        public GarmentTypeItemsService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<List<GarmentTypeItems>> GetBreakdownLinesByGarmentTypeAsync(int garmentTypeId)
        {
            // FIXED RELATIONAL HOOK: Directly queries the table using the true parent key integer ID!
            return await _apparelProDbContext.GarmentTypeItems
                .Where(g => g.GarmentTypeId == garmentTypeId)
                .OrderBy(g => g.StockCode)
                .ThenBy(g => g.ItemCode)
                .ToListAsync();
        }

        public async Task<bool> SaveBreakdownLineAsync(int garmentTypeId, string stockCode, string itemCode, string unit, decimal quantity)
        {
            stockCode = stockCode.Trim();
            itemCode = itemCode.Trim();
            unit = unit.Trim().ToUpper();

            // 1. VERIFY PARENT INTEGRITY BOUNDS: Ensure the parent GarmentType row exists in the database
            var parentTypeExists = await _apparelProDbContext.GarmentTypes
                .AnyAsync(t => t.Id == garmentTypeId);

            if (!parentTypeExists)
            {
                throw new InvalidOperationException($"Garment Configuration Mismatch: Target Garment Type Identity ID '{garmentTypeId}' does not exist in the master reference tables.");
            }

            // 2. GLOBAL MATRIX IDENTITY CHECK: Validate against OrderItem (od_itm) catalog
            var itemExistsInCatalog = await _apparelProDbContext.OrderItems
                .AnyAsync(i => i.StockCode == stockCode && i.ItemCode == itemCode);

            if (!itemExistsInCatalog)
            {
                throw new InvalidOperationException($"Validation Intercept: Material Item Code '{stockCode}/{itemCode}' is not registered inside the global reference catalog dictionary.");
            }

            // 3. PESSIMISTIC MERGE DATA STRATEGY
            var existingLine = await _apparelProDbContext.GarmentTypeItems
                .FirstOrDefaultAsync(g => g.GarmentTypeId == garmentTypeId && g.StockCode == stockCode && g.ItemCode == itemCode);

            if (existingLine == null)
            {
                var newLine = new GarmentTypeItems
                {
                    GarmentTypeId = garmentTypeId, // Maps perfectly to your database foreign key column!
                    StockCode = stockCode,
                    ItemCode = itemCode,
                    Unit = unit,
                    Quantity = quantity
                };
                await _apparelProDbContext.GarmentTypeItems.AddAsync(newLine);
            }
            else
            {
                existingLine.Unit = unit;
                existingLine.Quantity = quantity;
                _apparelProDbContext.GarmentTypeItems.Update(existingLine);
            }

            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBreakdownLineAsync(int garmentTypeId, string stockCode, string itemCode)
        {
            stockCode = stockCode.Trim();
            itemCode = itemCode.Trim();

            var targetLine = await _apparelProDbContext.GarmentTypeItems
                .FirstOrDefaultAsync(g => g.GarmentTypeId == garmentTypeId && g.StockCode == stockCode && g.ItemCode == itemCode);

            if (targetLine != null)
            {
                _apparelProDbContext.GarmentTypeItems.Remove(targetLine);
                await _apparelProDbContext.SaveChangesAsync();
            }

            return true;
        }
    }
}

