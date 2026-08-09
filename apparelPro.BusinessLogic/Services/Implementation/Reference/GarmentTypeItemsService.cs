using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeItemsService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    // Replicates OD_ITM1.PRG (Update) / OD_ITM2.PRG (List) - Reference Files >
    // B. Order Management > G. Type / Item in legacy (RF_MENU.PRG).
    // Item Code validation/description lookups go against StockItems (not
    // OrderItems) - StockItems is the catalog the frontend picker
    // (GetMaterialCatalogAsync) actually sources from, so this has to match or
    // a valid picker selection would fail here with "Invalid Item Code".
    public class GarmentTypeItemsService : IGarmentTypeItemsService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public GarmentTypeItemsService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<List<GarmentTypeItemServiceModel>> GetGarmentTypeItemsAsync(int garmentTypeId)
        {
            var rows = await _apparelProDbContext.GarmentTypeItems
                .AsNoTracking()
                .Where(g => g.GarmentTypeId == garmentTypeId)
                .OrderBy(g => g.StockCode).ThenBy(g => g.ItemCode)
                .ToListAsync();

            if (rows.Count == 0) return new List<GarmentTypeItemServiceModel>();

            var garmentTypeName = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => t.Id == garmentTypeId)
                .Select(t => t.TypeName)
                .FirstOrDefaultAsync() ?? "";

            var stockCodes = rows.Select(r => r.StockCode).Distinct().ToList();
            var itemDescriptions = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(i => stockCodes.Contains(i.StockCode))
                .ToDictionaryAsync(i => i.StockCode + "|" + i.ItemCode, i => i.Description);

            return rows.Select(r => new GarmentTypeItemServiceModel
            {
                Id = r.Id,
                GarmentTypeId = r.GarmentTypeId,
                GarmentTypeName = garmentTypeName,
                StockCode = r.StockCode,
                ItemCode = r.ItemCode,
                ItemDescription = itemDescriptions.TryGetValue(r.StockCode + "|" + r.ItemCode, out var desc) ? desc : "",
                Unit = r.Unit,
                Quantity = r.Quantity,
            }).ToList();
        }

        public async Task<GarmentTypeItemServiceModel> SaveGarmentTypeItemAsync(SaveGarmentTypeItemServiceModel request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var stockCode = (request.StockCode ?? "").Trim();
            var itemCode = (request.ItemCode ?? "").Trim();
            var unit = (request.Unit ?? "").Trim().ToUpper();

            var garmentType = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.GarmentTypeId);
            if (garmentType == null)
                throw new InvalidOperationException($"Garment Type '{request.GarmentTypeId}' was not found in the Garment Type master.");

            if (string.IsNullOrWhiteSpace(stockCode) || string.IsNullOrWhiteSpace(itemCode))
                throw new InvalidOperationException("Stock Code and Item Code are both required.");

            var itemExists = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .AnyAsync(i => i.StockCode == stockCode && i.ItemCode == itemCode);
            if (!itemExists)
                throw new InvalidOperationException($"Invalid Item Code: '{stockCode}/{itemCode}' was not found in the Item catalog.");

            if (string.IsNullOrWhiteSpace(unit))
                throw new InvalidOperationException("Unit is required.");
            var unitExists = await _apparelProDbContext.Units.AsNoTracking().AnyAsync(u => u.Code == unit);
            if (!unitExists)
                throw new InvalidOperationException($"Unit Code '{unit}' was not found in the Unit reference file.");

            if (request.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            var existing = await _apparelProDbContext.GarmentTypeItems
                .FirstOrDefaultAsync(g => g.GarmentTypeId == request.GarmentTypeId && g.StockCode == stockCode && g.ItemCode == itemCode);

            if (existing == null)
            {
                existing = new GarmentTypeItems
                {
                    GarmentTypeId = request.GarmentTypeId,
                    StockCode = stockCode,
                    ItemCode = itemCode,
                    Unit = unit,
                    Quantity = request.Quantity,
                };
                await _apparelProDbContext.GarmentTypeItems.AddAsync(existing);
            }
            else
            {
                existing.Unit = unit;
                existing.Quantity = request.Quantity;
            }

            await _apparelProDbContext.SaveChangesAsync();

            var itemDescription = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(i => i.StockCode == stockCode && i.ItemCode == itemCode)
                .Select(i => i.Description)
                .FirstOrDefaultAsync() ?? "";

            return new GarmentTypeItemServiceModel
            {
                Id = existing.Id,
                GarmentTypeId = existing.GarmentTypeId,
                GarmentTypeName = garmentType.TypeName,
                StockCode = existing.StockCode,
                ItemCode = existing.ItemCode,
                ItemDescription = itemDescription,
                Unit = existing.Unit,
                Quantity = existing.Quantity,
            };
        }

        public async Task<bool> DeleteGarmentTypeItemAsync(int garmentTypeId, string stockCode, string itemCode)
        {
            stockCode = (stockCode ?? "").Trim();
            itemCode = (itemCode ?? "").Trim();

            var existing = await _apparelProDbContext.GarmentTypeItems
                .FirstOrDefaultAsync(g => g.GarmentTypeId == garmentTypeId && g.StockCode == stockCode && g.ItemCode == itemCode);
            if (existing == null) return false;

            _apparelProDbContext.GarmentTypeItems.Remove(existing);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
