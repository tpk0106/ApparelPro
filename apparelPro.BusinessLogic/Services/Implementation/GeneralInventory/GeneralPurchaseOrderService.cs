using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_PORD1.PRG/GI_FPO.PRG - "PURCHASE ORDER ENTRY
    // (General)". A master-detail upsert (new P/O gets an allocated number via
    // NoteType "GPO"; an existing P/O number can be re-opened and its lines replaced),
    // unlike the ledger-driven note types elsewhere in General Inventory - a P/O commits
    // no stock movement of its own (GRN is what receives against it, already built).
    // Legacy's three "Continue...?" warning dialogs (re-order level / max stock /
    // re-order quantity) are non-blocking in legacy itself, so they're surfaced here as
    // advisory Warnings on the commit result rather than a hard validation failure.
    public class GeneralPurchaseOrderService : IGeneralPurchaseOrderService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralPurchaseOrderService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<GeneralPoCommitResultServiceModel> CommitGeneralPurchaseOrderAsync(
            GeneralPoHeaderServiceModel header,
            List<GeneralPoLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Purchase Order cannot be committed without line items.");

            header.SupplierCode = header.SupplierCode.Trim();
            header.BasisCode = header.BasisCode.Trim().ToUpper();
            header.CurrencyCode = header.CurrencyCode.Trim().ToUpper();

            if (!int.TryParse(header.SupplierCode, out var supplierCodeInt) ||
                !await _apparelProDbContext.Suppliers.AsNoTracking().AnyAsync(s => s.SupplierCode == supplierCodeInt))
                throw new InvalidOperationException($"Invalid Supplier Code '{header.SupplierCode}'.");

            if (!await _apparelProDbContext.Basis.AsNoTracking().AnyAsync(b => b.Code == header.BasisCode))
                throw new InvalidOperationException($"Invalid Basis Code '{header.BasisCode}'.");

            if (!await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == header.CurrencyCode))
                throw new InvalidOperationException($"Invalid Currency Code '{header.CurrencyCode}'.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string poNumber;
                if (header.IsNewPurchaseOrder)
                {
                    poNumber = await _sharedService.GenerateNextDocumentNumberAsync("GPO");
                }
                else
                {
                    poNumber = header.PoNumber.Trim();
                    if (string.IsNullOrEmpty(poNumber))
                        throw new InvalidOperationException("P/O Number is required when editing an existing P/O.");

                    var exists = await _apparelProDbContext.GeneralPurchaseOrders.AsNoTracking().AnyAsync(h => h.PoNumber == poNumber);
                    if (!exists)
                        throw new InvalidOperationException($"P/O No. '{poNumber}' does not exist.");
                }

                var warnings = new List<string>();

                foreach (var line in lines)
                {
                    line.StoreCode = line.StoreCode.Trim().ToUpper();
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    if (line.OrderedQuantity <= 0)
                        throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                    var generalMaster = await _apparelProDbContext.GeneralStockMasters
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.StoreCode == line.StoreCode && m.ItemCode == line.ItemCode);
                    if (generalMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in Item Master File for Stores '{line.StoreCode}'.");

                    decimal orderedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.OrderedQuantity);

                    if (generalMaster.QtyInHand <= generalMaster.ReorderLevel)
                        warnings.Add($"Item '{line.ItemCode}': Re-order Level not yet reached.");
                    if (orderedInMasterUnit + generalMaster.QtyInHand > generalMaster.MaxStock)
                        warnings.Add($"Item '{line.ItemCode}': Quantity exceeds Maximum Stock.");
                    if (orderedInMasterUnit > generalMaster.ReorderQuantity)
                        warnings.Add($"Item '{line.ItemCode}': Quantity exceeds Re-order Quantity.");
                    else if (orderedInMasterUnit < generalMaster.ReorderQuantity)
                        warnings.Add($"Item '{line.ItemCode}': Quantity is below Re-order Quantity.");

                    var existingDetail = await _apparelProDbContext.GeneralPurchaseOrderDetails
                        .FirstOrDefaultAsync(d => d.PoNumber == poNumber && d.StoreCode == line.StoreCode && d.ItemCode == line.ItemCode);

                    if (existingDetail != null)
                    {
                        existingDetail.RefNo = line.RefNo?.Trim();
                        existingDetail.Unit = line.Unit;
                        existingDetail.OrderedQuantity = line.OrderedQuantity;
                        existingDetail.Price = line.Price;
                        existingDetail.ExpectedDate = line.ExpectedDate.HasValue ? DateOnly.FromDateTime(line.ExpectedDate.Value) : null;
                        existingDetail.Balance = line.OrderedQuantity;
                        _apparelProDbContext.GeneralPurchaseOrderDetails.Update(existingDetail);
                    }
                    else
                    {
                        await _apparelProDbContext.GeneralPurchaseOrderDetails.AddAsync(new GeneralPurchaseOrderDetails
                        {
                            PoNumber = poNumber,
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            RefNo = line.RefNo?.Trim(),
                            Unit = line.Unit,
                            OrderedQuantity = line.OrderedQuantity,
                            Price = line.Price,
                            ExpectedDate = line.ExpectedDate.HasValue ? DateOnly.FromDateTime(line.ExpectedDate.Value) : null,
                            Balance = line.OrderedQuantity,
                        });
                    }
                }

                var poHeader = await _apparelProDbContext.GeneralPurchaseOrders.FirstOrDefaultAsync(h => h.PoNumber == poNumber);
                if (poHeader == null)
                {
                    await _apparelProDbContext.GeneralPurchaseOrders.AddAsync(new GeneralPurchaseOrder
                    {
                        PoNumber = poNumber,
                        SupplierCode = header.SupplierCode,
                        OrderDate = DateOnly.FromDateTime(header.OrderDate),
                        OrderTime = TimeOnly.FromDateTime(DateTime.Now),
                        BasisCode = header.BasisCode,
                        CurrencyCode = header.CurrencyCode,
                        ProformaInvoiceNo = header.ProformaInvoiceNo?.Trim(),
                        ProformaInvoiceDate = header.ProformaInvoiceDate.HasValue ? DateOnly.FromDateTime(header.ProformaInvoiceDate.Value) : null,
                        UserId = username.Trim().ToUpper(),
                    });
                }
                else
                {
                    poHeader.SupplierCode = header.SupplierCode;
                    poHeader.OrderDate = DateOnly.FromDateTime(header.OrderDate);
                    poHeader.BasisCode = header.BasisCode;
                    poHeader.CurrencyCode = header.CurrencyCode;
                    poHeader.ProformaInvoiceNo = header.ProformaInvoiceNo?.Trim();
                    poHeader.ProformaInvoiceDate = header.ProformaInvoiceDate.HasValue ? DateOnly.FromDateTime(header.ProformaInvoiceDate.Value) : null;
                    poHeader.UserId = username.Trim().ToUpper();
                    _apparelProDbContext.GeneralPurchaseOrders.Update(poHeader);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new GeneralPoCommitResultServiceModel { PoNumber = poNumber, Warnings = warnings };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<GeneralPOServiceModel> GetGeneralPurchaseOrderAsync(string poNumber)
        {
            poNumber = poNumber.Trim();

            var poHeader = await _apparelProDbContext.GeneralPurchaseOrders.AsNoTracking().FirstOrDefaultAsync(h => h.PoNumber == poNumber);
            if (poHeader == null)
                throw new KeyNotFoundException($"P/O No. '{poNumber}' not found.");

            var details = await _apparelProDbContext.GeneralPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => d.PoNumber == poNumber)
                .ToListAsync();

            return new GeneralPOServiceModel
            {
                Header = new GeneralPoHeaderServiceModel
                {
                    PoNumber = poHeader.PoNumber,
                    IsNewPurchaseOrder = false,
                    SupplierCode = poHeader.SupplierCode,
                    OrderDate = poHeader.OrderDate.HasValue ? poHeader.OrderDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.Today,
                    BasisCode = poHeader.BasisCode ?? "",
                    CurrencyCode = poHeader.CurrencyCode ?? "",
                    ProformaInvoiceNo = poHeader.ProformaInvoiceNo,
                    ProformaInvoiceDate = poHeader.ProformaInvoiceDate.HasValue ? poHeader.ProformaInvoiceDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                },
                Lines = details.Select(d => new GeneralPoLineItemServiceModel
                {
                    StoreCode = d.StoreCode,
                    ItemCode = d.ItemCode,
                    RefNo = d.RefNo,
                    Unit = d.Unit,
                    OrderedQuantity = d.OrderedQuantity,
                    Price = d.Price,
                    ExpectedDate = d.ExpectedDate.HasValue ? d.ExpectedDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                }).ToList(),
            };
        }

        public async Task<GeneralPoPrintDetailsServiceModel> GetGeneralPoPrintDetailsAsync(string poNumber)
        {
            poNumber = poNumber.Trim();

            var poHeader = await _apparelProDbContext.GeneralPurchaseOrders.AsNoTracking().FirstOrDefaultAsync(h => h.PoNumber == poNumber);
            if (poHeader == null)
                throw new KeyNotFoundException($"P/O No. '{poNumber}' not found.");

            var details = await _apparelProDbContext.GeneralPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => d.PoNumber == poNumber)
                .ToListAsync();

            if (details.Count == 0)
                throw new KeyNotFoundException($"No Details found for P/O No. '{poNumber}'.");

            int supplierCodeInt = int.TryParse(poHeader.SupplierCode, out var parsed) ? parsed : 0;
            var supplier = await _apparelProDbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierCode == supplierCodeInt);

            var itemCodes = details.Select(d => d.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralPoPrintDetailsServiceModel
            {
                Header = new GeneralPoPrintHeaderServiceModel
                {
                    PoNumber = poNumber,
                    OrderDate = poHeader.OrderDate.HasValue ? poHeader.OrderDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    SupplierCode = supplierCodeInt,
                    SupplierName = supplier?.Name ?? "",
                    SupplierAddress = "",
                    CurrencyCode = poHeader.CurrencyCode ?? "",
                    ProformaInvoiceNo = poHeader.ProformaInvoiceNo,
                    ProformaInvoiceDate = poHeader.ProformaInvoiceDate.HasValue ? poHeader.ProformaInvoiceDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    PrintedOn = DateTime.Now,
                },
                Lines = details.Select(d => new GeneralPoPrintLineServiceModel
                {
                    RefNo = d.RefNo ?? "",
                    ItemCode = d.ItemCode,
                    Description = descriptions.GetValueOrDefault(d.ItemCode, ""),
                    Unit = d.Unit,
                    OrderedQuantity = d.OrderedQuantity,
                    Price = d.Price,
                    ExpectedDate = d.ExpectedDate.HasValue ? d.ExpectedDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                }).ToList(),
            };
        }
    }
}
