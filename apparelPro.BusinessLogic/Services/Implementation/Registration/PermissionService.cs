using apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Registration
{
    public class PermissionService : IPermissionService
    {
        private readonly UserIdentityDbContext _userIdentityDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public PermissionService(UserIdentityDbContext userIdentityDbContext, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            if (userIdentityDbContext == null)
            {
                throw new ArgumentNullException(nameof(userIdentityDbContext));
            }
            if (roleManager == null)
            {
                throw new ArgumentNullException(nameof(roleManager));
            }
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            _userIdentityDbContext = userIdentityDbContext;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<List<PermissionServiceModel>> GetPermissionCatalogAsync()
        {
            var permissions = await _userIdentityDbContext.Permissions
                .OrderBy(p => p.Category).ThenBy(p => p.DisplayName)
                .ToListAsync();

            return _mapper.Map<List<PermissionServiceModel>>(permissions);
        }

        public async Task<List<RolePermissionMatrixRoleServiceModel>> GetRolePermissionMatrixAsync()
        {
            var roles = await _userIdentityDbContext.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            var rolePermissions = await _userIdentityDbContext.RolePermissions
                .Include(rp => rp.Permission)
                .ToListAsync();

            var result = new List<RolePermissionMatrixRoleServiceModel>();
            foreach (var role in roles)
            {
                if (role.Id == null || role.Name == null)
                {
                    continue;
                }

                var grantedKeys = rolePermissions
                    .Where(rp => rp.RoleId == role.Id && rp.Permission != null)
                    .Select(rp => rp.Permission!.Key)
                    .ToList();

                result.Add(new RolePermissionMatrixRoleServiceModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    GrantedPermissionKeys = grantedKeys
                });
            }

            return result;
        }

        public async Task UpdateRolePermissionsAsync(UpdateRolePermissionsServiceModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{model.RoleId}' was not found.");
            }

            var permissionIdsByKey = await _userIdentityDbContext.Permissions
                .Where(p => model.PermissionKeys.Contains(p.Key))
                .ToDictionaryAsync(p => p.Key, p => p.Id);

            using var transaction = await _userIdentityDbContext.Database.BeginTransactionAsync();

            var existingGrants = await _userIdentityDbContext.RolePermissions
                .Where(rp => rp.RoleId == model.RoleId)
                .ToListAsync();
            _userIdentityDbContext.RolePermissions.RemoveRange(existingGrants);

            foreach (var permissionKey in model.PermissionKeys)
            {
                if (!permissionIdsByKey.TryGetValue(permissionKey, out var permissionId))
                {
                    continue;
                }

                _userIdentityDbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = model.RoleId,
                    PermissionId = permissionId
                });
            }

            await _userIdentityDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<Dictionary<string, List<string>>> GetPermissionKeysByRoleNameAsync()
        {
            var roles = await _userIdentityDbContext.Roles.ToListAsync();
            var rolePermissions = await _userIdentityDbContext.RolePermissions
                .Include(rp => rp.Permission)
                .ToListAsync();

            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var role in roles)
            {
                if (role.Name == null)
                {
                    continue;
                }

                result[role.Name] = rolePermissions
                    .Where(rp => rp.RoleId == role.Id && rp.Permission != null)
                    .Select(rp => rp.Permission!.Key)
                    .ToList();
            }

            return result;
        }

        public async Task SeedDefaultCatalogAsync()
        {
            foreach (var entry in DefaultCatalog)
            {
                var existingPermission = await _userIdentityDbContext.Permissions
                    .FirstOrDefaultAsync(p => p.Key == entry.Key);

                if (existingPermission == null)
                {
                    existingPermission = new Permission
                    {
                        Key = entry.Key,
                        DisplayName = entry.DisplayName,
                        Category = entry.Category,
                        Description = entry.Description
                    };
                    _userIdentityDbContext.Permissions.Add(existingPermission);
                    await _userIdentityDbContext.SaveChangesAsync();
                }

                foreach (var roleName in entry.DefaultRoleNames)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        // Role doesn't exist yet in this environment - skip gracefully.
                        // ApparelProSeedController.LoadData is what creates roles; this
                        // seed never fabricates one (Zero-Assumption Boundary Rule).
                        continue;
                    }

                    var alreadyGranted = await _userIdentityDbContext.RolePermissions
                        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == existingPermission.Id);

                    if (!alreadyGranted)
                    {
                        _userIdentityDbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = existingPermission.Id
                        });
                    }
                }
            }

            await _userIdentityDbContext.SaveChangesAsync();
        }

        // Catalog keys retired by a controller cutover pass and explicitly safe to
        // delete - nothing references them anymore. Add to this list only when a key
        // is being permanently replaced, never as a way to "temporarily" hide one.
        private static readonly string[] ObsoleteCatalogKeys = { "bank", "garment-type", "currency", "currency-exchange", "department", "port-destination", "item-feature", "address" };

        public async Task RemoveObsoleteCatalogEntriesAsync()
        {
            var obsoletePermissions = await _userIdentityDbContext.Permissions
                .Where(p => ObsoleteCatalogKeys.Contains(p.Key))
                .ToListAsync();

            if (obsoletePermissions.Count == 0)
            {
                return;
            }

            var obsoleteIds = obsoletePermissions.Select(p => p.Id).ToList();

            var obsoleteGrants = await _userIdentityDbContext.RolePermissions
                .Where(rp => obsoleteIds.Contains(rp.PermissionId))
                .ToListAsync();

            _userIdentityDbContext.RolePermissions.RemoveRange(obsoleteGrants);
            _userIdentityDbContext.Permissions.RemoveRange(obsoletePermissions);

            await _userIdentityDbContext.SaveChangesAsync();
        }

        // ---------------------------------------------------------------------------
        // Role-name groupings, reused across the catalog below. These mirror the exact
        // role literal strings ApparelProSeedController.LoadData creates.
        // ---------------------------------------------------------------------------
        private static readonly string[] OrderwiseInventoryStandardRoles = { "Inventory", "Merchandiser", "Merchandiser Manager", "Order Entry Operator", "Administrator" };
        private static readonly string[] MerchandisingOnlyRoles = { "Merchandiser", "Merchandiser Manager" };
        private static readonly string[] StoreManagementOnlyRoles = { "Store Manager", "Administrator" };
        private static readonly string[] AdministratorOnlyRoles = { "Administrator" };
        private static readonly string[] BuyersWidenedRoles = { "Inventory", "Merchandiser", "Merchandiser Manager", "Order Entry Operator", "Store Manager", "Administrator" };
        private static readonly string[] StyleApprovalOnlyRoles = { "Merchandiser Manager" };
        // Trim Sheet Report (2026-08-07): base viewing roles. A superset of
        // MerchandisingOnlyRoles so Merchandising Manager/Executive Director - who also need
        // to see the profit-gated section below - aren't blocked from the report entirely.
        // Administrator included per this file's established convention (every other role
        // array above defaults Administrator in) - originally omitted, which silently denied
        // Administrator accounts access to Trim Sheet Report even after this catalog entry was
        // seeded (FIXED 2026-08-07).
        private static readonly string[] TrimSheetReportRoles = { "Merchandiser", "Merchandiser Manager", "Merchandising Manager", "Executive Director", "Administrator" };
        // Estimated Profit section only - NOT wired as a policy. Documented here for the
        // catalog audit trail; TrimSheetReportController checks these same 3 roles inline via
        // User.IsInRole(...), matching how MaterialConsumptionController's own higher-authority
        // check is done (a hardcoded inline check, not a second [Authorize] policy) rather than
        // referencing this private array across layers. Mirrors legacy's separate
        // access('trimprof') gate on top of general Trim Sheet viewing. Per explicit project
        // decision (2026-08-07): Merchandiser Manager, Merchandising Manager, Executive Director.
        private static readonly string[] ColorSizeBreakdownBulkSaveRoles = { "Merchandiser", "Merchandiser Manager", "Order Entry Operator" };
        // Production Control module: three real roles created by
        // ApparelProSeedController.LoadData (2026-08-16) - "Production", referenced here
        // originally, was never an actual AspNetRoles entry (just a config string in
        // appsettings.json's PolicyManager:Reports:Roles for the older raw-role system),
        // so it silently granted nobody. View widened to the entry-level operator role
        // plus Merchandiser/Merchandiser Manager/Administrator, mirroring how
        // OrderwiseInventoryStandardRoles includes Order Entry Operator for view; manage
        // restricted to the two manager-level roles plus Merchandiser Manager, excluding
        // the entry operator, matching the Bank/GarmentType -manage precedent.
        private static readonly string[] ProductionViewRoles = { "Production Manager", "Production Entry Operator", "Factory Manager", "Merchandiser", "Merchandiser Manager", "Administrator" };
        private static readonly string[] ProductionManageRoles = { "Production Manager", "Factory Manager", "Merchandiser Manager" };

        // Import/Export Documentation module (Commercial Invoice, Company Address
        // Setup, and future document types e.g. Certificate of Origin/GSP) - a
        // merchandising/shipping function in legacy (IE_MENU.PRG), not touched by
        // Inventory or Order Entry Operator roles, so no separate wider -view set.
        private static readonly string[] ImportExportRoles = { "Merchandiser", "Merchandiser Manager", "Administrator" };

        // Home dashboard spans Order Management + Orderwise Inventory +
        // Production Progress, so its role set is the union of the roles
        // that can already see each of those areas individually - anyone
        // who could reach at least one of the three tabs through its own
        // screen can also reach the combined summary.
        private static readonly string[] DashboardViewRoles = { "Production Manager", "Production Entry Operator", "Factory Manager", "Merchandiser", "Merchandiser Manager", "Inventory", "Order Entry Operator", "Store Manager", "Administrator" };

        // ---------------------------------------------------------------------------
        // Default permission catalog - reproduces, section by section, the raw-Roles /
        // AccessPolicies-driven authorization your controllers already enforce today
        // (confirmed directly against each controller's current [Authorize] attributes
        // on 2026-07-28, not assumed). Seeding this only ever ADDS RolePermission rows -
        // it never revokes anything - so running it changes no enforced behavior yet
        // (no controller has been cut over to [Authorize(Policy = "...")] as of Stage 2).
        //
        // Known nuances deliberately NOT modeled here yet (deferred to the controller
        // cutover pass, since that's where per-endpoint precision actually matters):
        //  - AddressController mixes 5 endpoints under the "Merchandising" named policy
        //    (untouched, separate mechanism) with 2 raw-Roles endpoints; "address" here
        //    reflects only the 2 raw-Roles endpoints.
        //  - BuyersController's "list" endpoint was deliberately widened to 6 roles
        //    while Delete/Update stay at the narrower 4-role OrderwiseInventoryStandard -
        //    modeled as two distinct permissions since that split is real, current
        //    behavior, not an inconsistency to paper over.
        //
        // RESOLVED 2026-07-30: BankController and GarmentTypeController used to stack a
        // class-level [Authorize] with a narrower method-level [Authorize], which
        // ASP.NET Core enforces as an AND - silently over-restricting several endpoints
        // (some had no method-level attribute at all and just inherited the class-level
        // gate). Cut over to two policy keys per controller (-view / -manage) below,
        // with the class-level [Authorize] removed entirely so each endpoint's own
        // attribute is authoritative. The retired "bank"/"garment-type" keys are
        // removed via RemoveObsoleteCatalogEntriesAsync (see below), not left dangling.
        //
        // RESOLVED 2026-07-30 (batch 2): same cutover applied across the remaining
        // Reference Data controllers (Currency, CurrencyExchange, Department,
        // PortDestination, ItemFeature, Country, Unit, UnitConversion, Basis,
        // CurrencyConversion). This batch also closed several endpoints that had NO
        // [Authorize] attribute at all and were reachable by any authenticated user:
        // CountryController (POST/PUT/PATCH/DELETE + 2 of its 5 GETs - the worst gap
        // found), PortDestinationController.AddPortDestinationAsync +
        // GetPortDestinationByCodeAndCountryCodeAsync, ItemFeatureController
        // .GetItemFeatureByFeatureCodeAsync + AddFeatureAsync,
        // DepartmentController.GetDepartmentsLookup, and
        // UnitConversionController's single GET endpoint. It also removed the buggy
        // class-level [Authorize("RegisteredUser")] (resolves to "Merchandiser
        // Manager" only) from CurrencyExchangeController, UnitController,
        // BasisController and CurrencyConversionController, which was silently
        // AND-stacking with those controllers' method-level attributes.
        // ---------------------------------------------------------------------------
        private static readonly IReadOnlyList<PermissionCatalogEntry> DefaultCatalog = new List<PermissionCatalogEntry>
        {
            new("gin", "Goods Issue Note (GIN)", "Orderwise Inventory", "GINController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("grn", "Goods Received Note (GRN)", "Orderwise Inventory", "GRNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("gtn", "Goods Transfer Note (GTN)", "Orderwise Inventory", "GTNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("dtn", "Direct Goods Transfer Note (DTN)", "Orderwise Inventory", "DTNController - class-level [Authorize]. Mirror image of GTN but allows the destination item code to differ from the source (legacy IN_DTN1.PRG/IN_DTN2.PRG).", OrderwiseInventoryStandardRoles),
            new("rtn", "Goods Return Note (RTN)", "Orderwise Inventory", "RTNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("srn", "Supplier Return Note (SRN)", "Orderwise Inventory", "SRNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("strn", "Stores Requisition Note (STRN)", "Orderwise Inventory", "STRNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("dgn", "Damaged Goods Note (DGN)", "Orderwise Inventory", "DGNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("san", "Store Adjustment Note (SAN)", "Orderwise Inventory", "SANController - class-level [Authorize]", StoreManagementOnlyRoles),
            new("ain", "Additional Issue Note (AIN)", "Orderwise Inventory", "AINController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("arn", "Additional Goods Receipts Note (ARN)", "Orderwise Inventory", "ARNController - class-level [Authorize]. Mirror image of AIN - receives processed goods back from a Sub-Contractor, revaluing the order-level master price.", OrderwiseInventoryStandardRoles),
            new("stock-valuation-report", "Stock Valuation Report (Orderwise)", "Reports", "StockValuationReportController - class-level [Authorize]. Per-Buyer/Order valuation grouped by Stock Type, reads OrderwiseStockMaster's live running totals.", OrderwiseInventoryStandardRoles),
            new("stock-valuation-monthly-report", "Stock Valuation Report - Monthly (Orderwise)", "Reports", "StockValuationMonthlyReportController - class-level [Authorize]. Item-level (Buyer/Order-agnostic) GR/4I totals for a date range.", OrderwiseInventoryStandardRoles),
            new("orderwise-stock-status-report", "Stock Status Report (Orderwise)", "Reports", "StockStatusReportController - class-level [Authorize]. Per-Buyer/Order current-snapshot listing straight off OrderwiseStock.", OrderwiseInventoryStandardRoles),
            new("orderwise-transaction-list-report", "List of Transactions (Orderwise)", "Reports", "TransactionListReportController - class-level [Authorize]. Flat OrderwiseStockTransactions log for a date range, optional type/item-code-prefix filter.", OrderwiseInventoryStandardRoles),
            new("item-wise-stock-balance-report", "Item-wise Stock Balances (Orderwise)", "Reports", "ItemWiseStockBalanceController - class-level [Authorize]. System-wide (every Buyer/Order) balance listing for a 6-char Stock+Item code range, excludes zero-balance items.", OrderwiseInventoryStandardRoles),
            new("raw-material-control-sheet-report", "Raw Material Control Sheet (Orderwise)", "Reports", "RawMaterialControlSheetController - class-level [Authorize]. Per-Buyer/Order material consumption vs Order/Received/Issued/Balance, from StyleMaterialConsumptionLedger + OrderwiseStockMaster.", OrderwiseInventoryStandardRoles),
            new("orderwise-stock-summary-report", "Stock Summary Report - Basis wise (Orderwise)", "Reports", "StockSummaryReportController - class-level [Authorize]. System-wide Stock Type/Store grouped valuation in two currencies, reads OrderwiseStock's live QtyInHand (no replay needed).", OrderwiseInventoryStandardRoles),
            new("orderwise-grn-listing-report", "GRN Listing (Orderwise)", "Reports", "GrnListingReportController - class-level [Authorize]. Unifies legacy's separate Date-Wise/Buyer-Order-Wise GRN listing screens into one flexible report.", OrderwiseInventoryStandardRoles),
            new("general-strn", "Stores Requisition Note (General)", "General Inventory", "GeneralSTRNController - class-level [Authorize]", OrderwiseInventoryStandardRoles),
            new("general-gin", "Goods Issue Note (General)", "General Inventory", "GeneralGINController - class-level [Authorize]. The minimum-stock override is a narrower inline role check (\"Merchandiser Manager\") in the controller, not this policy.", OrderwiseInventoryStandardRoles),
            new("general-grn", "Goods Received Note (General)", "General Inventory", "GeneralGRNController - class-level [Authorize]. The maximum-stock override needs no extra role - matches legacy's ungated Yes/No prompt.", OrderwiseInventoryStandardRoles),
            new("general-gtn", "Goods Transfer Note (General)", "General Inventory", "GeneralGTNController - class-level [Authorize]. Atomic transfer between two General stores, no override needed.", OrderwiseInventoryStandardRoles),
            new("general-ogtn", "Goods Transfer Note (Orders)", "General Inventory", "OrderGTNController - class-level [Authorize]. Bridges General Inventory and Orderwise Inventory for a specific Buyer/Order.", OrderwiseInventoryStandardRoles),
            new("general-rtn", "Goods Return Note (General)", "General Inventory", "GeneralRTNController - class-level [Authorize]. Material returning from a Department back into a General store.", OrderwiseInventoryStandardRoles),
            new("general-dgn", "Damaged Goods Note (General)", "General Inventory", "GeneralDGNController - class-level [Authorize]. Writes off stock at a General store as damaged.", OrderwiseInventoryStandardRoles),
            new("general-srtn", "Supplier Return Note (General)", "General Inventory", "GeneralSRTNController - class-level [Authorize]. Regular or Damaged stock returned to a supplier.", OrderwiseInventoryStandardRoles),
            new("general-po", "Purchase Order Entry (General)", "General Inventory", "GeneralPOController - class-level [Authorize]. Master-detail P/O entry, no stock movement of its own.", OrderwiseInventoryStandardRoles),
            new("general-stock-master", "Stock Master Entry (General)", "General Inventory", "GeneralStockMasterController - class-level [Authorize]. Creates/edits GeneralStockMaster rows - required before any other General Inventory note can move that item.", OrderwiseInventoryStandardRoles),
            new("general-san", "Stock Adjustment Note (General)", "General Inventory", "GeneralSANController - class-level [Authorize]. Sets QtyInHand to an absolute stock-take count, not an add/subtract delta.", OrderwiseInventoryStandardRoles),
            new("general-stock-status-report", "Stock Status Report (General)", "Reports", "GeneralStockStatusReportController - class-level [Authorize]. Chronological replay of GeneralStockTransactions for a Store/Month, no stored gi_monst equivalent.", OrderwiseInventoryStandardRoles),
            new("general-stock-movement-report", "Stock Movement Report (General)", "Reports", "GeneralStockMovementReportController - class-level [Authorize]. Per-item transaction-by-transaction ledger with running balance, chronological replay of GeneralStockTransactions.", OrderwiseInventoryStandardRoles),
            new("general-stock-valuation-report", "Stock Valuation Report (General)", "Reports", "GeneralStockValuationReportController - class-level [Authorize]. Current-snapshot listing straight off GeneralStockMasters for a Store/Item range - no transaction replay.", OrderwiseInventoryStandardRoles),
            new("general-stock-reorder-report", "Stock Re-order Report (General)", "Reports", "GeneralStockReorderReportController - class-level [Authorize]. Current-snapshot listing of GeneralStockMasters rows where QtyInHand <= ReorderLevel for a Store.", OrderwiseInventoryStandardRoles),
            new("general-transaction-list-report", "List of Transactions (General)", "Reports", "GeneralTransactionListReportController - class-level [Authorize]. Flat GeneralStockTransactions log for a date range, optional type/item-prefix filter, no running balance.", OrderwiseInventoryStandardRoles),
            new("general-purchase-order-list-report", "List of P/O's (General)", "Reports", "GeneralPurchaseOrderListReportController - class-level [Authorize]. Flat GeneralPurchaseOrders listing for a date range.", OrderwiseInventoryStandardRoles),
            new("general-stock-summary-report", "Stock Summary Report (General)", "Reports", "GeneralStockSummaryReportController - class-level [Authorize]. Stock Type/Store grouped valuation summary in two currencies, chronological replay of GeneralStockTransactions for month-end balances.", OrderwiseInventoryStandardRoles),
            new("general-grn-listing-report", "GRN Listing (General)", "Reports", "GeneralGrnListingReportController - class-level [Authorize]. TransactionTypeCode '0G' listing for a date range, optional Store/Supplier filters (unifies legacy's separate Date-Wise/Supplier-Wise screens).", OrderwiseInventoryStandardRoles),
            new("stock-movement-report", "Stock Movement Report", "Reports", "StockMovementReportController (ApparelPro.WebApi/Reports/) - class-level [Authorize]. Missed by the Stage 1 audit script (different folder) - still on a raw role string today.", OrderwiseInventoryStandardRoles),
            new("stock-movement-item-report", "Stock Movement Report (for an Item)", "Reports", "StockMovementItemReportController (ApparelPro.WebApi/Reports/) - class-level [Authorize(Policy = \"stock-movement-item-report\")].", OrderwiseInventoryStandardRoles),
            new("bank-view", "Bank Master - View / Lookup", "Reference Data", "BankController GET endpoints (list, list/{code})", OrderwiseInventoryStandardRoles),
            new("bank-manage", "Bank Master - Add / Update", "Reference Data", "BankController POST/PUT endpoints", MerchandisingOnlyRoles),
            new("basis-view", "Basis Master - View / Lookup", "Reference Data", "BasisController GET endpoints (list, list/{code})", OrderwiseInventoryStandardRoles),
            new("season-view", "Season Master - View / Lookup", "Reference Data", "SeasonController GET endpoints (list, list/{code}) - built for the Season dropdown on Order Confirmation and the Year/Season Wise Orders report.", OrderwiseInventoryStandardRoles),
            new("season-manage", "Season Master - Add / Update / Delete", "Reference Data", "SeasonController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("basis-manage", "Basis Master - Add / Update / Delete", "Reference Data", "BasisController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("buyers-view", "Buyers - View / Lookup", "Reference Data", "BuyersController.GetBuyersAsync ('list') - deliberately widened so every note-type's Buyer dropdown works", BuyersWidenedRoles),
            new("buyers-manage", "Buyers - Add / Update / Delete", "Reference Data", "BuyersController Delete/Update endpoints", OrderwiseInventoryStandardRoles),
            new("color-size-breakdown", "Color/Size Breakdown Details", "Reference Data", "ColorSizeBreakdownDetailsController's 2 guarded endpoints", MerchandisingOnlyRoles),
            new("color-size-breakdown-bulk-save", "Color/Size Breakdown - Bulk Save", "Reference Data", "ColorSizeBreakdownDetailsController.BulkSaveDetailsAsync - wider grant includes Order Entry Operator, unlike the controller's other 2 guarded endpoints", ColorSizeBreakdownBulkSaveRoles),
            new("country-view", "Country Master - View / Lookup", "Reference Data", "CountryController GET endpoints (list, list/{code}, does-exist, paging, filter)", OrderwiseInventoryStandardRoles),
            new("country-manage", "Country Master - Add / Update / Delete", "Reference Data", "CountryController POST/PUT/PATCH/DELETE endpoints - previously had NO authorization at all", MerchandisingOnlyRoles),
            new("currency-view", "Currency Master - View / Lookup", "Reference Data", "CurrencyController GET endpoints (list, list/{code}, does-exist)", OrderwiseInventoryStandardRoles),
            new("currency-manage", "Currency Master - Add / Update / Delete", "Reference Data", "CurrencyController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("currency-conversion-view", "Currency Conversion - View / Lookup", "Reference Data", "CurrencyConversionController GET endpoints (list, list/{fromCurrency}/{toCurrency}) - previously the controller had only a single, broken GET endpoint; POST/PUT/DELETE were added 2026-08-09, hence the new -manage key below", OrderwiseInventoryStandardRoles),
            new("currency-conversion-manage", "Currency Conversion - Add / Update / Delete", "Reference Data", "CurrencyConversionController POST/PUT/DELETE endpoints - added 2026-08-09 (didn't exist before; the service methods all threw NotImplementedException)", MerchandisingOnlyRoles),
            new("currency-exchange-view", "Currency Exchange Rates - View / Lookup", "Reference Data", "CurrencyExchangeController GET endpoints (list, byDate, list/{baseCurrency}, list/{baseCurrency}/{quoteCurrency}/{date})", OrderwiseInventoryStandardRoles),
            new("currency-exchange-manage", "Currency Exchange Rates - Add / Update / Delete", "Reference Data", "CurrencyExchangeController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("department-view", "Department Master - View / Lookup", "Reference Data", "DepartmentController's 2 GET endpoints (list, lookup)", OrderwiseInventoryStandardRoles),
            new("garment-type-view", "Garment Type Master - View / Lookup", "Reference Data", "GarmentTypeController GET endpoints (list, list/all, list/{id})", OrderwiseInventoryStandardRoles),
            new("garment-type-manage", "Garment Type Master - Add / Update", "Reference Data", "GarmentTypeController POST/PUT/PATCH endpoints", MerchandisingOnlyRoles),
            new("garment-type-item-view", "Garment Type wise Item Requirements - View / Lookup", "Reference Data", "GarmentTypeItemsController GET endpoint (list) - RF_MENU.PRG > B. Order Management > G. Type / Item", OrderwiseInventoryStandardRoles),
            new("garment-type-item-manage", "Garment Type wise Item Requirements - Add / Update / Delete", "Reference Data", "GarmentTypeItemsController POST/DELETE endpoints", MerchandisingOnlyRoles),
            new("item-feature-view", "Item Feature Master - View / Lookup", "Reference Data", "ItemFeatureController GET endpoints (list, list/{featureCode})", OrderwiseInventoryStandardRoles),
            new("item-feature-manage", "Item Feature Master - Add / Update / Delete", "Reference Data", "ItemFeatureController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("order-item-feature-view", "Order Item Feature - View / Lookup", "Reference Data", "OrderItemFeatureController GET endpoints (list, list/{stockCode}/{itemCode}, does-exist)", OrderwiseInventoryStandardRoles),
            new("order-item-feature-manage", "Order Item Feature - Add / Update / Delete", "Reference Data", "OrderItemFeatureController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("material-consumption", "Material Consumption", "Order Management", "MaterialConsumptionController - class-level [Authorize]", MerchandisingOnlyRoles),
            new("garment-additional-cost-view", "Additional Costs per Garment - View / Lookup / Report", "Order Management", "GarmentAdditionalCostController GET endpoints (list, report/details, report/pdf)", OrderwiseInventoryStandardRoles),
            new("garment-additional-cost-manage", "Additional Costs per Garment - Add / Update / Delete", "Order Management", "GarmentAdditionalCostController POST/DELETE endpoints", MerchandisingOnlyRoles),
            new("sub-contract-view", "Sub Contracts - View / Lookup", "Order Management", "SubContractController GET endpoints (list) - built 2026-08-09 from od_subc1.prg", OrderwiseInventoryStandardRoles),
            new("sub-contract-manage", "Sub Contracts - Add / Update / Delete", "Order Management", "SubContractController POST/DELETE endpoints", MerchandisingOnlyRoles),
            new("trim-sheet-report", "Trim Sheet Report", "Reports", "TrimSheetReportController - class-level [Authorize]. The Estimated Profit section within it has its own narrower role check (TrimSheetProfitRoles) inline in the controller, not this policy.", TrimSheetReportRoles),
            new("order-detail-report", "Order Detail Report", "Reports", "OrderDetailReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy. Same viewing audience as Trim Sheet Report.", TrimSheetReportRoles),
            new("color-size-report", "Colour/Size Report", "Reports", "ColorSizeReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy. Same viewing audience as Trim Sheet Report / Order Detail Report.", TrimSheetReportRoles),
            new("purchase-order-list-report", "List of P/O's Report", "Reports", "PurchaseOrderListReportController - all three endpoints ([HttpGet(\"po-numbers\")], [HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("outstanding-purchase-order-list-report", "List of Outstanding P/O's Report", "Reports", "OutstandingPurchaseOrderListReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("scheduled-shipments-report", "Scheduled Shipments Report", "Reports", "ScheduledShipmentsReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_RSHP1.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("shipment-status-report", "Shipment status Report", "Reports", "ShipmentStatusReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_SHPST.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("year-season-orders-report", "Year/Season Wise Orders Report", "Reports", "YearSeasonOrdersReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_RPO2.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("pending-events-report", "Pending Events Report", "Reports", "PendingEventsReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_EVPND.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("stock-arrival-status-report", "Stock Arrival Status Report", "Reports", "StockArrivalStatusReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_STARV.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("cost-of-production-report", "Cost of Production Report", "Reports", "CostOfProductionReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_FCOST.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("order-quota-detail-report", "Order/Quota Detail Report", "Reports", "OrderQuotaDetailReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_ROQ1.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("post-order-cost-sheet-report", "Post Order Cost Sheet Report", "Reports", "PostOrderCostSheetReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_PCOST.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("monthly-actual-shipments-report", "Monthly Actual Shipments Report", "Reports", "MonthlyActualShipmentsReportController - both endpoints ([HttpGet(\"details\")], [HttpGet(\"pdf\")]) carry this policy - migrated from OD_ACTSP.PRG. Same viewing audience as the other Order Management reports.", TrimSheetReportRoles),
            new("port-destination-view", "Port / Destination Master - View / Lookup", "Reference Data", "PortDestinationController GET endpoints (list, list/{code}/{countryCode})", OrderwiseInventoryStandardRoles),
            new("port-destination-manage", "Port / Destination Master - Add / Update / Delete", "Reference Data", "PortDestinationController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("style-details", "Style Details", "Order Management", "StyleDetailsController.GetStyleDetailsAsync ('list')", OrderwiseInventoryStandardRoles),
            new("style-approval", "Style Event Approval", "Order Management", "StyleApprovalController.ApproveEvents", StyleApprovalOnlyRoles),
            new("unit-view", "Unit Master - View / Lookup", "Reference Data", "UnitController GET endpoints (list, list/{code}, does-unit-exist)", OrderwiseInventoryStandardRoles),
            new("unit-manage", "Unit Master - Add / Update", "Reference Data", "UnitController POST/PUT endpoints", MerchandisingOnlyRoles),
            new("unit-conversion-view", "Unit Conversion - View / Lookup", "Reference Data", "UnitConversionController's single GET endpoint - previously had NO authorization at all", OrderwiseInventoryStandardRoles),
            new("address-view", "Address Master - View / Lookup (Buyer-scoped)", "Reference Data", "AddressController's 5 GET endpoints - Buyer-address only today; Supplier-address support is a separate planned feature (Address entity has no SupplierCode column yet)", OrderwiseInventoryStandardRoles),
            new("address-manage", "Address Master - Add / Update / Delete (Buyer-scoped)", "Reference Data", "AddressController POST/PUT/DELETE endpoints - AddAddressAsync and the generic UpdateAddressAsync previously had NO authorization at all; DeleteBuyerAddressAsync previously granted Inventory/Order Entry Operator delete access via OrderwiseInventoryStandard, narrowed to Merchandiser/Merchandiser Manager to match every other -manage key", MerchandisingOnlyRoles),
            new("additional-cost-view", "Additional Cost Master - View / Lookup", "Reference Data", "AdditionalCostController GET endpoints (list, list/{code}, does-exist)", OrderwiseInventoryStandardRoles),
            new("additional-cost-manage", "Additional Cost Master - Add / Update / Delete", "Reference Data", "AdditionalCostController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("sub-contractor-view", "Sub Contractor Master - View / Lookup", "Reference Data", "SubContractorController GET endpoints (list, list/{code}, does-exist) - built 2026-08-09 as a prerequisite for AIN", OrderwiseInventoryStandardRoles),
            new("sub-contractor-manage", "Sub Contractor Master - Add / Update / Delete", "Reference Data", "SubContractorController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("supplier-view", "Supplier Master - View / Lookup", "Reference Data", "SupplierController GET endpoints (list, list/{SupplierCode}, suppliers-lookup) - used by Material Consumption; controller already carried [Authorize(Policy = \"supplier-view\")] but this catalog key was never seeded, so no role had a granted RolePermission row and every role was denied", OrderwiseInventoryStandardRoles),
            new("supplier-manage", "Supplier Master - Add / Update / Delete", "Reference Data", "SupplierController POST/PUT/DELETE endpoints - same missing-catalog-entry gap as supplier-view", MerchandisingOnlyRoles),
            new("stock-view", "Stock Reference - View / Lookup", "Reference Data", "StockController GET endpoints (list, list/{stockCode}, does-exist) - RF_MENU.PRG > C. Inventory Control > A. Stock Reference", OrderwiseInventoryStandardRoles),
            new("stock-manage", "Stock Reference - Add / Update / Delete", "Reference Data", "StockController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("order-item-catalog-view", "Order Items Catalog - View / Lookup", "Reference Data", "OrderItemCatalogController GET endpoints (list, list/{stockCode}/{itemCode}, does-exist) - the od_itm Stock/Item master list", OrderwiseInventoryStandardRoles),
            new("order-item-catalog-manage", "Order Items Catalog - Add / Update / Delete", "Reference Data", "OrderItemCatalogController POST/PUT/DELETE endpoints", MerchandisingOnlyRoles),
            new("security-administration", "Security Administration", "System", "SecurityController.Revoke", AdministratorOnlyRoles),

            // production control (Phase 1 - master data, PR_MENU.PRG > Production Details)
            new("production-line-view", "Production Line Master - View / Lookup", "Production Control", "ProductionLineController GET endpoints (list, list/{lineCode}) - migrated from OD_LINE1.PRG", ProductionViewRoles),
            new("production-line-manage", "Production Line Master - Add / Update / Delete", "Production Control", "ProductionLineController POST/PUT/DELETE endpoints", ProductionManageRoles),
            new("operation-view", "Operation Master - View / Lookup", "Production Control", "OperationController GET endpoints (list, list/{operationCode}) - migrated from PR_MOP11.PRG", ProductionViewRoles),
            new("operation-manage", "Operation Master - Add / Update / Delete", "Production Control", "OperationController POST/PUT/DELETE endpoints", ProductionManageRoles),
            new("non-productive-hour-view", "Non-Productive Hour Code - View / Lookup", "Production Control", "NonProductiveHourCodeController GET endpoints (list, list/{code}) - migrated from PR_NPH1.PRG", ProductionViewRoles),
            new("non-productive-hour-manage", "Non-Productive Hour Code - Add / Update / Delete", "Production Control", "NonProductiveHourCodeController POST/PUT/DELETE endpoints", ProductionManageRoles),
            new("machine-type-view", "Machine Type Master - View / Lookup", "Production Control", "MachineTypeController GET endpoints (list, list/{code}) - migrated from PR_MMCH1.PRG", ProductionViewRoles),
            new("machine-type-manage", "Machine Type Master - Add / Update / Delete", "Production Control", "MachineTypeController POST/PUT/DELETE endpoints", ProductionManageRoles),
            new("garment-component-view", "Garment Component Master - View / Lookup", "Production Control", "GarmentComponentController GET endpoints (list, list/{componentCode}) - migrated from PR_CMP1.PRG", ProductionViewRoles),
            new("garment-component-manage", "Garment Component Master - Add / Update / Delete", "Production Control", "GarmentComponentController POST/PUT/DELETE endpoints", ProductionManageRoles),
            new("employee-view", "Employee Master - View / Lookup", "Production Control", "EmployeeController GET endpoints (list, list/{employeeCode}) - minimal stand-in for PAY_EMPL, which legacy Production only ever read from a separate Payroll app folder (..\\wpay\\pay_empl); no Payroll module exists yet so Employee is owned here for now", ProductionViewRoles),
            new("employee-manage", "Employee Master - Add / Update / Delete", "Production Control", "EmployeeController POST/PUT/DELETE endpoints", ProductionManageRoles),

            // production control (Phase 2 - Style Component/Operation Breakdown, PR_OPD1.PRG / PR_OPD2.PRG)
            new("style-component-breakdown-view", "Style Component Breakdown - View", "Production Control", "StyleComponentBreakdownController GET by-style endpoint - migrated from PR_OPD1.PRG", ProductionViewRoles),
            new("style-component-breakdown-manage", "Style Component Breakdown - Bulk Save", "Production Control", "StyleComponentBreakdownController bulk-save endpoint", ProductionManageRoles),
            new("style-operation-breakdown-view", "Style Operation Breakdown - View", "Production Control", "StyleOperationBreakdownController GET by-style endpoint - migrated from PR_OPD2.PRG", ProductionViewRoles),
            new("style-operation-breakdown-manage", "Style Operation Breakdown - Seed / Bulk Save", "Production Control", "StyleOperationBreakdownController seed-from-template and bulk-save endpoints (includes the line-balancing recalculation)", ProductionManageRoles),
            new("component-operation-template-view", "Component/Operation Template - View / Lookup", "Production Control", "ComponentOperationTemplateController GET endpoints (list, by-component) - migrated from PR_MOP21.PRG", ProductionViewRoles),
            new("component-operation-template-manage", "Component/Operation Template - Add / Update / Delete", "Production Control", "ComponentOperationTemplateController POST/PUT/DELETE endpoints - migrated from PR_MOP22.PRG", ProductionManageRoles),

            // production control (Phase 3 - Production Line Allocation, PR_ESTM1.PRG / PR_ESTL1.PRG)
            new("holiday-view", "Production Calendar (Holidays) - View / Lookup", "Production Control", "HolidayController GET endpoint - migrated from CALENDER.PRG", ProductionViewRoles),
            new("holiday-manage", "Production Calendar (Holidays) - Add / Delete", "Production Control", "HolidayController POST/DELETE endpoints", ProductionManageRoles),
            new("production-line-allocation-view", "Production Line Allocation - View", "Production Control", "ProductionLineAllocationController GET by-shipment endpoint - migrated from PR_ESTM1.PRG", ProductionViewRoles),
            new("production-line-allocation-manage", "Production Line Allocation - Manual / Automatic Allocate / Delete", "Production Control", "ProductionLineAllocationController manual/automatic/delete endpoints", ProductionManageRoles),
            new("estimated-production-line-allocation-view", "Est. Production Line Allocation - View", "Production Control", "EstimatedProductionLineAllocationController GET endpoint - migrated from PR_ESTL1.PRG", ProductionViewRoles),
            new("estimated-production-line-allocation-manage", "Est. Production Line Allocation - Manual / Automatic Allocate / Delete", "Production Control", "EstimatedProductionLineAllocationController manual/automatic/delete endpoints", ProductionManageRoles),

            // production control (Phase 4 - Daily Production Time Ticket, PR_DPTT1.PRG)
            new("daily-production-time-ticket-view", "Daily Production Time Ticket - View", "Production Control", "DailyProductionTimeTicketController GET endpoint - migrated from PR_DPTT1.PRG", ProductionViewRoles),
            new("daily-production-time-ticket-manage", "Daily Production Time Ticket - Bulk Save", "Production Control", "DailyProductionTimeTicketController bulk-save endpoint (includes the per-employee efficiency recalculation)", ProductionManageRoles),

            // production control (Phase 5a - Estimated Production Entry, PR_ESTD1.PRG)
            new("estimated-production-entry-view", "Estimated Production Entry - View", "Production Control", "EstimatedProductionEntryController GET by-line endpoint - migrated from PR_ESTD1.PRG", ProductionViewRoles),
            new("estimated-production-entry-manage", "Estimated Production Entry - Bulk Save", "Production Control", "EstimatedProductionEntryController bulk-save endpoint", ProductionManageRoles),

            // production control (Phase 5b - Section master, prerequisite for Actual Production Entry / PR_DPRO2.PRG)
            new("section-view", "Section Master - View / Lookup", "Production Control", "SectionController GET endpoints (list, list/all) - migrated from OD_SECT1.PRG", ProductionViewRoles),
            new("section-manage", "Section Master - Add / Update / Delete", "Production Control", "SectionController POST/PUT/DELETE endpoints - migrated from OD_SECT2.PRG", ProductionManageRoles),

            // production control (Phase 5c - Actual Production Entry, PR_DPRO2.PRG)
            new("daily-production-entry-view", "Actual Production Entry - View", "Production Control", "DailyProductionEntryController GET by-date endpoint - migrated from PR_DPRO2.PRG", ProductionViewRoles),
            new("daily-production-entry-manage", "Actual Production Entry - Bulk Save", "Production Control", "DailyProductionEntryController bulk-save endpoint", ProductionManageRoles),

            // home dashboard (Floor pulse)
            new("dashboard-view", "Home Dashboard - View", "Dashboard", "DashboardController GET endpoints (current-style, production-progress)", DashboardViewRoles),

            // production reports (Reports -> C. Production Summary (Daily), PR_DPROD.PRG)
            new("production-summary-daily-report-view", "Production Summary (Daily) Report - View", "Production Control", "ProductionSummaryDailyReportController GET endpoint - migrated from PR_DPROD.PRG", ProductionViewRoles),

            // production reports (Reports -> A. Production Schedule, PR_MSCHD.PRG)
            new("production-schedule-report-view", "Production Schedule Report - View", "Production Control", "ProductionScheduleReportController GET endpoint - migrated from PR_MSCHD.PRG", ProductionViewRoles),

            // production reports (Reports -> B. Production Summary (Monthly), PR_MPROD.PRG)
            new("production-summary-monthly-report-view", "Production Summary (Monthly) Report - View", "Production Control", "ProductionSummaryMonthlyReportController GET endpoint - migrated from PR_MPROD.PRG", ProductionViewRoles),
            new("production-summary-monthly-overview-report-view", "Production Summary (Monthly) Simplified Report - View", "Production Control", "ProductionSummaryMonthlyOverviewReportController GET endpoint - not a legacy screen, simplified companion to PR_MPROD.PRG", ProductionViewRoles),

            // production reports (Reports -> D. Production Summary (Style Wise), PR_MPRO1.PRG)
            new("production-summary-style-wise-report-view", "Production Summary (Style Wise) Report - View", "Production Control", "ProductionSummaryStyleWiseReportController GET endpoint - migrated from PR_MPRO1.PRG", ProductionViewRoles),
            new("production-summary-style-wise-detailed-report-view", "Production Summary (Style Wise) Detailed Report - View", "Production Control", "ProductionSummaryStyleWiseDetailedReportController GET endpoint - per-line breakdown companion to PR_MPRO1.PRG", ProductionViewRoles),

            // production reports (Reports -> E. Line Production Summary, PR_LPROD.PRG)
            new("line-production-summary-report-view", "Line Production Summary Report - View", "Production Control", "LineProductionSummaryReportController GET endpoint - migrated from PR_LPROD.PRG", ProductionViewRoles),

            // production reports (Reports -> F. Operation Breakdown, PR_REP1.PRG)
            new("operation-breakdown-report-view", "Operation Breakdown Report - View", "Production Control", "OperationBreakdownReportController GET endpoint - migrated from PR_REP1.PRG", ProductionViewRoles),

            // production reports (Reports -> G. Manpower Requirement, PR_REP3.PRG)
            new("manpower-requirement-report-view", "Manpower Requirement Report - View", "Production Control", "ManpowerRequirementReportController GET endpoint - migrated from PR_REP3.PRG", ProductionViewRoles),

            // production reports (Reports -> H. Employee Efficiency (Daily), PR_EEF1.PRG)
            new("daily-employee-efficiency-report-view", "Daily Employee Efficiency Report - View", "Production Control", "DailyEmployeeEfficiencyReportController GET endpoint - migrated from PR_EEF1.PRG", ProductionViewRoles),

            // production reports (Reports -> I. Employee Efficiency (Monthly), PR_REP4.PRG)
            new("monthly-employee-efficiency-report-view", "Monthly Employee Efficiency Report - View", "Production Control", "MonthlyEmployeeEfficiencyReportController GET endpoint - migrated from PR_REP4.PRG", ProductionViewRoles),

            // production reports (Reports -> J. Production Line Efficiency, PR_GRH1.PRG)
            new("line-efficiency-report-view", "Production Line Efficiency Report - View", "Production Control", "LineEfficiencyReportController GET endpoint - migrated from PR_GRH1.PRG", ProductionViewRoles),

            // production reports (Reports -> K. Estimated Production Schedule, PR_ESTL2.PRG)
            new("estimated-production-schedule-report-view", "Estimated Production Schedule Report - View", "Production Control", "EstimatedProductionScheduleReportController GET endpoint - migrated from PR_ESTL2.PRG", ProductionViewRoles),

            // production reports (Reports -> L. Production Analysis Summary (for Style), PR_MPRO2.PRG)
            new("production-analysis-summary-report-view", "Production Analysis Summary Report - View", "Production Control", "ProductionAnalysisSummaryReportController GET endpoint - migrated from PR_MPRO2.PRG", ProductionViewRoles),

            // production control (Production Progress Graph, PR_PROG.PRG)
            new("production-progress-graph-view", "Production Progress Graph - View", "Production Control", "ProductionProgressGraphController GET endpoint - migrated from PR_PROG.PRG", ProductionViewRoles),

            // production control (End of Production Confirmation, PR_ENDPR.PRG)
            new("end-of-production-confirmation-view", "End of Production Confirmation - View", "Production Control", "EndOfProductionConfirmationController GET endpoint - migrated from PR_ENDPR.PRG", ProductionViewRoles),
            new("end-of-production-confirmation-manage", "End of Production Confirmation - Confirm", "Production Control", "EndOfProductionConfirmationController confirm endpoint - sets Style.ProductionEndDate", ProductionManageRoles),

            // import/export documentation
            new("company-address-view", "Company Address Setup - View", "Import / Export Documentation", "CompanyAddressController GET endpoint - migrated from ie_setup", ImportExportRoles),
            new("company-address-manage", "Company Address Setup - Add / Update / Delete", "Import / Export Documentation", "CompanyAddressController PUT/DELETE endpoints", ImportExportRoles),
            new("commercial-invoice-view", "Commercial Invoice - View", "Import / Export Documentation", "CommercialInvoiceController GET endpoints (list, detail, print/pdf) - migrated from IE_COIN1.PRG", ImportExportRoles),
            new("commercial-invoice-manage", "Commercial Invoice - Add / Update / Delete", "Import / Export Documentation", "CommercialInvoiceController PUT/DELETE endpoints", ImportExportRoles),
            new("certificate-of-origin-view", "Certificate of Origin - View", "Import / Export Documentation", "CertificateOfOriginController GET endpoint - matches the real Ceylon Chamber of Commerce EXP 5 form, not legacy ie_co", ImportExportRoles),
            new("certificate-of-origin-manage", "Certificate of Origin - Add / Update / Delete", "Import / Export Documentation", "CertificateOfOriginController PUT/DELETE endpoints", ImportExportRoles),
            new("letter-of-credit-view", "L/C Form - View", "Import / Export Documentation", "LetterOfCreditController GET endpoint - matches legacy ie_lc/ie_lc2 (IE_LCBC1.PRG), one flat schema shared across BOC/SCB/Peoples Bank formats", ImportExportRoles),
            new("letter-of-credit-manage", "L/C Form - Add / Update / Delete", "Import / Export Documentation", "LetterOfCreditController PUT/DELETE endpoints", ImportExportRoles),
            new("customs-declaration-view", "CUSDEC I/II - View", "Import / Export Documentation", "CustomsDeclarationController GET endpoint - migrated from ie_cusd1-4 (IE_CUSD1.PRG/IE_CUSD2.PRG)", ImportExportRoles),
            new("customs-declaration-manage", "CUSDEC I/II - Add / Update / Delete", "Import / Export Documentation", "CustomsDeclarationController PUT/DELETE endpoints", ImportExportRoles),
            new("clearance-office-view", "Clearance/Frontier Office - View", "Import / Export Documentation", "ClearanceOfficeController GET endpoint - migrated from ie_coff, shared by CUSDEC's Clearance Office and Frontier Office fields", ImportExportRoles),
            new("clearance-office-manage", "Clearance/Frontier Office - Add / Update / Delete", "Import / Export Documentation", "ClearanceOfficeController PUT/DELETE endpoints", ImportExportRoles),
            new("payment-term-view", "Payment Terms - View", "Import / Export Documentation", "PaymentTermController GET endpoint - migrated from ie_pay", ImportExportRoles),
            new("payment-term-manage", "Payment Terms - Add / Update / Delete", "Import / Export Documentation", "PaymentTermController PUT/DELETE endpoints", ImportExportRoles),
            new("transport-mode-view", "Transport Mode (TRPT) - View", "Import / Export Documentation", "TransportModeController GET endpoint - migrated from ie_tran", ImportExportRoles),
            new("transport-mode-manage", "Transport Mode (TRPT) - Add / Update / Delete", "Import / Export Documentation", "TransportModeController PUT/DELETE endpoints", ImportExportRoles),
            new("document-type-view", "Document Type - View", "Import / Export Documentation", "DocumentTypeController GET endpoint - migrated from ie_docu (composite DocNo+DocType)", ImportExportRoles),
            new("document-type-manage", "Document Type - Add / Update / Delete", "Import / Export Documentation", "DocumentTypeController PUT/DELETE endpoints", ImportExportRoles),
            new("duty-tax-code-view", "Duty/Tax Code - View", "Import / Export Documentation", "DutyTaxCodeController GET endpoint - migrated from ie_dtax", ImportExportRoles),
            new("duty-tax-code-manage", "Duty/Tax Code - Add / Update / Delete", "Import / Export Documentation", "DutyTaxCodeController PUT/DELETE endpoints", ImportExportRoles),
            new("tax-base-code-view", "Tax Base Code - View", "Import / Export Documentation", "TaxBaseCodeController GET endpoint - migrated from ie_tbas", ImportExportRoles),
            new("tax-base-code-manage", "Tax Base Code - Add / Update / Delete", "Import / Export Documentation", "TaxBaseCodeController PUT/DELETE endpoints", ImportExportRoles),
            new("agreement-code-view", "Agreement Code - View", "Import / Export Documentation", "AgreementCodeController GET endpoint - migrated from ie_agre", ImportExportRoles),
            new("agreement-code-manage", "Agreement Code - Add / Update / Delete", "Import / Export Documentation", "AgreementCodeController PUT/DELETE endpoints", ImportExportRoles),
            new("commodity-code-view", "Commodity Code - View", "Import / Export Documentation", "CommodityCodeController GET endpoint - migrated from ie_comm", ImportExportRoles),
            new("commodity-code-manage", "Commodity Code - Add / Update / Delete", "Import / Export Documentation", "CommodityCodeController PUT/DELETE endpoints", ImportExportRoles),
            new("customs-procedure-code-view", "Customs Procedure Code (CPC) - View", "Import / Export Documentation", "CustomsProcedureCodeController GET endpoint - migrated from ie_cpro", ImportExportRoles),
            new("customs-procedure-code-manage", "Customs Procedure Code (CPC) - Add / Update / Delete", "Import / Export Documentation", "CustomsProcedureCodeController PUT/DELETE endpoints", ImportExportRoles),
            new("packing-list-view", "Packing List - View", "Import / Export Documentation", "PackingListController GET endpoint - migrated from ie_pack1/ie_pack2/ie_pack3 (IE_PACK1.PRG)", ImportExportRoles),
            new("packing-list-manage", "Packing List - Add / Update", "Import / Export Documentation", "PackingListController PUT endpoint", ImportExportRoles),
        };

        private sealed record PermissionCatalogEntry(string Key, string DisplayName, string Category, string? Description, string[] DefaultRoleNames);
    }
}
