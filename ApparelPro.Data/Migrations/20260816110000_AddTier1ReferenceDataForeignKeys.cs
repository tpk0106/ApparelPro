using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <summary>
    /// Tier 1 of the missing-relationships audit (2026-08-16): formalizes 25
    /// reference-data relationships that today are enforced only by matching
    /// column values, as real FOREIGN KEY constraints. All 25 passed the
    /// orphan-row check run against the live database beforehand.
    ///
    /// Two prerequisite unique indexes are added first: Basis.Code and
    /// Units.Code are both referenced by other tables via their business
    /// "Code" column, not their surrogate "Id" primary key, so SQL Server
    /// needs a unique index on Code before anything can reference it.
    ///
    /// Deliberately EXCLUDED from this migration (tracked separately):
    ///   - PurchaseOrders.BasisCode -> Basis.Code (3 orphan rows)
    ///   - OrderwiseStockTransactions.DepartmentCode -> Departments.DepartmentCode (17 orphan rows)
    ///   - StyleMaterialCostProfiles.CurrencyCode -> Currencies.Code (7 orphan rows)
    ///   - PurchaseOrderHeaders/StyleMaterialConsumptionLedger/StyleMaterialCostProfiles.SupplierCode
    ///     -> Suppliers.SupplierCode (varchar vs int type mismatch - needs its own decision)
    ///
    /// All FKs use ReferentialAction.Restrict, matching the existing convention for
    /// reference-data relationships already in this schema (FK_StockItems_Stocks_StockCode,
    /// FK_GarmentTypeItems_GarmentTypes_GarmentTypeId, FK_ProductionLines_Currencies_CurrencyCode,
    /// FK_ProductionLines_Units_UnitCode, FK_EventMasters_StylewiseEvents_EventCode all use
    /// Restrict - SQL Server generates this as ON DELETE NO ACTION; Cascade is reserved for true
    /// parent-owns-child relationships like Address->Bank/Buyer and ASP.NET Identity).
    ///
    /// FIX (2026-08-16, same day): the first run of this migration failed with a type-mismatch
    /// error on FK_PurchaseOrderHeaders_Currencies_CurrencyCode (Currencies.Code was nvarchar(3),
    /// PurchaseOrderHeaders.CurrencyCode was varchar(3) - SQL Server requires an exact match).
    /// The same nvarchar/varchar mismatch existed on 13 of these 25 relationships, all pointing at
    /// either Currencies.Code or Units.Code. Rather than widen 13 scattered consumer columns, this
    /// migration now narrows Currencies.Code and Units.Code to varchar(3) to match the convention
    /// used everywhere else (Stock.StockCode, SubContractor.Code, Department.DepartmentCode are all
    /// varchar). ProductionLines already has a live FK into both columns
    /// (FK_ProductionLines_Currencies_CurrencyCode, FK_ProductionLines_Units_UnitCode), so that FK
    /// and the PK_Currencies / AK_Units_Code constraints backing it are dropped and recreated around
    /// the type change. The failed first run left no partial state (confirmed via
    /// __EFMigrationsHistory - EF wraps the whole migration in one transaction).
    ///
    /// FIX 2 (2026-08-16): the retry hit "The model has pending changes" - Styles.Unit had no
    /// explicit column type and was implicitly resolving to Units.Code's old nvarchar(3) via EF's
    /// FK-type-inheritance convention. StyleConfig.cs now sets it explicitly to varchar(3), and this
    /// migration alters that column physically alongside the others.
    ///
    /// FIX 3 (2026-08-16): the next retry failed with "Cannot drop the index... because it does not
    /// exist" - a manual DropUniqueConstraint on AK_Units_Code conflicted with EF's own automatic
    /// index-preserving wrap around the Units.Code AlterColumn (EF auto-drops/recreates any plain
    /// index referencing an altered column, the same way it already handles
    /// IX_ProductionLines_CurrencyCode/UnitCode with no manual code needed).
    ///
    /// FIX 4 (2026-08-16): removing the manual drop (per FIX 3) then failed differently -
    /// "An explicit DROP INDEX is not allowed on index 'Units.AK_Units_Code'. It is being used for
    /// UNIQUE KEY constraint enforcement." AK_Units_Code is a real UNIQUE CONSTRAINT, not a plain
    /// index, so EF's auto-wrap (which only knows how to drop/recreate plain indexes) generated
    /// invalid SQL against it. Root cause: UnitConfig.cs declared Code via HasIndex().IsUnique(),
    /// but 12 separate FK declarations elsewhere in the codebase already call
    /// .HasPrincipalKey(u => u.Code), which requires Code to be a genuine EF key - so EF was already
    /// implicitly treating it as an alternate key (that's why the live constraint's name,
    /// AK_Units_Code, matches EF's own default alternate-key naming convention). UnitConfig.cs now
    /// declares this explicitly via HasAlternateKey instead of HasIndex().IsUnique(), which stops
    /// EF's auto-wrap from treating it as a plain index - and this migration goes back to explicit
    /// DropUniqueConstraint/AddUniqueConstraint around the Units.Code alter, same pattern as
    /// PK_Currencies, which EF has never auto-wrapped since keys (primary or alternate) aren't
    /// indexes from EF's perspective.
    /// </summary>
    public partial class AddTier1ReferenceDataForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- Narrow Currencies.Code / Units.Code from nvarchar(3) to varchar(3) ----
            // See FIX note above. ProductionLines' existing FK + the constraints backing it
            // have to be dropped before the type change and recreated after.
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLines_Currencies_CurrencyCode",
                table: "ProductionLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLines_Units_UnitCode",
                table: "ProductionLines");

            // AK_Units_Code is a genuine UNIQUE CONSTRAINT (confirmed live: is_unique_constraint = 1),
            // not a plain index - SQL Server refuses a bare DROP INDEX against it ("being used for
            // UNIQUE KEY constraint enforcement"). It must be dropped/recreated as a constraint, same
            // as PK_Currencies below. UnitConfig.cs now declares this via HasAlternateKey (not
            // HasIndex().IsUnique()), so EF's AlterColumn generator treats it like a key - not an
            // index - and won't also try to auto-wrap it, avoiding the double-drop seen earlier.
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Units_Code",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "ProductionLines",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "ProductionLines",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Currencies",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Units",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Units_Code",
                table: "Units",
                column: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLines_Currencies_CurrencyCode",
                table: "ProductionLines",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLines_Units_UnitCode",
                table: "ProductionLines",
                column: "UnitCode",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- Prerequisites: unique indexes on business keys ----
            migrationBuilder.CreateIndex(
                name: "IX_Basis_Code",
                table: "Basis",
                column: "Code",
                unique: true);

            // NOTE: no CreateIndex for Units.Code here - AK_Units_Code (recreated above)
            // already provides the unique constraint every Unit-referencing FK below needs.

            // ---- Addresses ----
            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CountryCode",
                table: "Addresses",
                column: "CountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Countries_CountryCode",
                table: "Addresses",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- Banks ----
            // Banks.CurrencyCode was nvarchar, matching Currency.Code's OLD type - it was correctly
            // "OK" in the original type audit, before the narrowing above flipped which side is the
            // mismatch (same pattern as Styles.Unit, PurchaseOrders.CurrencyCode/UnitCode below).
            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "Banks",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_CurrencyCode",
                table: "Banks",
                column: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_Currencies_CurrencyCode",
                table: "Banks",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- Currencies ----
            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CountryCode",
                table: "Currencies",
                column: "CountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Currencies_Countries_CountryCode",
                table: "Currencies",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- PurchaseOrderHeaders ----
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeaders_CurrencyCode",
                table: "PurchaseOrderHeaders",
                column: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderHeaders_Currencies_CurrencyCode",
                table: "PurchaseOrderHeaders",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- PurchaseOrders ----
            // PurchaseOrders.UnitCode / CurrencyCode were both nvarchar, matching Unit.Code /
            // Currency.Code's OLD type - correctly "OK" in the original audit, now mismatched by
            // the same narrowing-flips-the-verdict pattern as Banks.CurrencyCode above.
            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "PurchaseOrders",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "PurchaseOrders",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CountryCode",
                table: "PurchaseOrders",
                column: "CountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Countries_CountryCode",
                table: "PurchaseOrders",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_UnitCode",
                table: "PurchaseOrders",
                column: "UnitCode");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Units_UnitCode",
                table: "PurchaseOrders",
                column: "UnitCode",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CurrencyCode",
                table: "PurchaseOrders",
                column: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Currencies_CurrencyCode",
                table: "PurchaseOrders",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_GarmentType",
                table: "PurchaseOrders",
                column: "GarmentType");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_GarmentTypes_GarmentType",
                table: "PurchaseOrders",
                column: "GarmentType",
                principalTable: "GarmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ---- OrderwiseStockTransactions ----
            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_StockCode",
                table: "OrderwiseStockTransactions",
                column: "StockCode");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockTransactions_Stocks_StockCode",
                table: "OrderwiseStockTransactions",
                column: "StockCode",
                principalTable: "Stocks",
                principalColumn: "StockCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_SupplierCode",
                table: "OrderwiseStockTransactions",
                column: "SupplierCode");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockTransactions_Suppliers_SupplierCode",
                table: "OrderwiseStockTransactions",
                column: "SupplierCode",
                principalTable: "Suppliers",
                principalColumn: "SupplierCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_SubContractorCode",
                table: "OrderwiseStockTransactions",
                column: "SubContractorCode");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockTransactions_SubContractors_SubContractorCode",
                table: "OrderwiseStockTransactions",
                column: "SubContractorCode",
                principalTable: "SubContractors",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_Currency",
                table: "OrderwiseStockTransactions",
                column: "Currency");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockTransactions_Currencies_Currency",
                table: "OrderwiseStockTransactions",
                column: "Currency",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_Unit",
                table: "OrderwiseStockTransactions",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockTransactions_Units_Unit",
                table: "OrderwiseStockTransactions",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- OrderwiseStockMasters ----
            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockMasters_Currency",
                table: "OrderwiseStockMasters",
                column: "Currency");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockMasters_Currencies_Currency",
                table: "OrderwiseStockMasters",
                column: "Currency",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockMasters_Unit",
                table: "OrderwiseStockMasters",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStockMasters_Units_Unit",
                table: "OrderwiseStockMasters",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- OrderwiseStocks ----
            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStocks_Unit",
                table: "OrderwiseStocks",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderwiseStocks_Units_Unit",
                table: "OrderwiseStocks",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- GarmentTypeItems ----
            migrationBuilder.CreateIndex(
                name: "IX_GarmentTypeItems_StockCode",
                table: "GarmentTypeItems",
                column: "StockCode");

            migrationBuilder.AddForeignKey(
                name: "FK_GarmentTypeItems_Stocks_StockCode",
                table: "GarmentTypeItems",
                column: "StockCode",
                principalTable: "Stocks",
                principalColumn: "StockCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentTypeItems_Unit",
                table: "GarmentTypeItems",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_GarmentTypeItems_Units_Unit",
                table: "GarmentTypeItems",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- GarmentAdditionalCosts ----
            migrationBuilder.CreateIndex(
                name: "IX_GarmentAdditionalCosts_Currency",
                table: "GarmentAdditionalCosts",
                column: "Currency");

            migrationBuilder.AddForeignKey(
                name: "FK_GarmentAdditionalCosts_Currencies_Currency",
                table: "GarmentAdditionalCosts",
                column: "Currency",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentAdditionalCosts_Unit",
                table: "GarmentAdditionalCosts",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_GarmentAdditionalCosts_Units_Unit",
                table: "GarmentAdditionalCosts",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- Styles ----
            // Styles.Unit has no explicit column type override in StyleConfig.cs, so it was
            // resolved by convention to match Units.Code's old nvarchar(3) type. Now that
            // Units.Code is varchar(3), StyleConfig.cs was updated to explicitly match - this
            // physically narrows the column so FK_Styles_Units_Unit below doesn't hit the same
            // type-mismatch error the original PurchaseOrderHeaders FK did.
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "Styles",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Styles_Unit",
                table: "Styles",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_Styles_Units_Unit",
                table: "Styles",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- SubContracts ----
            migrationBuilder.CreateIndex(
                name: "IX_SubContracts_Currency",
                table: "SubContracts",
                column: "Currency");

            migrationBuilder.AddForeignKey(
                name: "FK_SubContracts_Currencies_Currency",
                table: "SubContracts",
                column: "Currency",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_SubContracts_Unit",
                table: "SubContracts",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_SubContracts_Units_Unit",
                table: "SubContracts",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- PartShipments ----
            migrationBuilder.CreateIndex(
                name: "IX_PartShipments_Unit",
                table: "PartShipments",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_PartShipments_Units_Unit",
                table: "PartShipments",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // ---- QuotaTransactions ----
            migrationBuilder.CreateIndex(
                name: "IX_QuotaTransactions_Unit",
                table: "QuotaTransactions",
                column: "Unit");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotaTransactions_Units_Unit",
                table: "QuotaTransactions",
                column: "Unit",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop every FK first (children before the unique-index prerequisites they depend on).
            migrationBuilder.DropForeignKey(name: "FK_Addresses_Countries_CountryCode", table: "Addresses");
            migrationBuilder.DropForeignKey(name: "FK_Banks_Currencies_CurrencyCode", table: "Banks");
            migrationBuilder.DropForeignKey(name: "FK_Currencies_Countries_CountryCode", table: "Currencies");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseOrderHeaders_Currencies_CurrencyCode", table: "PurchaseOrderHeaders");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseOrders_Countries_CountryCode", table: "PurchaseOrders");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseOrders_Units_UnitCode", table: "PurchaseOrders");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseOrders_Currencies_CurrencyCode", table: "PurchaseOrders");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseOrders_GarmentTypes_GarmentType", table: "PurchaseOrders");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockTransactions_Stocks_StockCode", table: "OrderwiseStockTransactions");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockTransactions_Suppliers_SupplierCode", table: "OrderwiseStockTransactions");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockTransactions_SubContractors_SubContractorCode", table: "OrderwiseStockTransactions");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockTransactions_Currencies_Currency", table: "OrderwiseStockTransactions");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockTransactions_Units_Unit", table: "OrderwiseStockTransactions");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockMasters_Currencies_Currency", table: "OrderwiseStockMasters");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStockMasters_Units_Unit", table: "OrderwiseStockMasters");
            migrationBuilder.DropForeignKey(name: "FK_OrderwiseStocks_Units_Unit", table: "OrderwiseStocks");
            migrationBuilder.DropForeignKey(name: "FK_GarmentTypeItems_Stocks_StockCode", table: "GarmentTypeItems");
            migrationBuilder.DropForeignKey(name: "FK_GarmentTypeItems_Units_Unit", table: "GarmentTypeItems");
            migrationBuilder.DropForeignKey(name: "FK_GarmentAdditionalCosts_Currencies_Currency", table: "GarmentAdditionalCosts");
            migrationBuilder.DropForeignKey(name: "FK_GarmentAdditionalCosts_Units_Unit", table: "GarmentAdditionalCosts");
            migrationBuilder.DropForeignKey(name: "FK_Styles_Units_Unit", table: "Styles");
            migrationBuilder.DropForeignKey(name: "FK_SubContracts_Currencies_Currency", table: "SubContracts");
            migrationBuilder.DropForeignKey(name: "FK_SubContracts_Units_Unit", table: "SubContracts");
            migrationBuilder.DropForeignKey(name: "FK_PartShipments_Units_Unit", table: "PartShipments");
            migrationBuilder.DropForeignKey(name: "FK_QuotaTransactions_Units_Unit", table: "QuotaTransactions");

            // Then every supporting index.
            migrationBuilder.DropIndex(name: "IX_Addresses_CountryCode", table: "Addresses");
            migrationBuilder.DropIndex(name: "IX_Banks_CurrencyCode", table: "Banks");
            migrationBuilder.DropIndex(name: "IX_Currencies_CountryCode", table: "Currencies");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrderHeaders_CurrencyCode", table: "PurchaseOrderHeaders");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrders_CountryCode", table: "PurchaseOrders");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrders_UnitCode", table: "PurchaseOrders");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrders_CurrencyCode", table: "PurchaseOrders");
            migrationBuilder.DropIndex(name: "IX_PurchaseOrders_GarmentType", table: "PurchaseOrders");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockTransactions_StockCode", table: "OrderwiseStockTransactions");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockTransactions_SupplierCode", table: "OrderwiseStockTransactions");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockTransactions_SubContractorCode", table: "OrderwiseStockTransactions");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockTransactions_Currency", table: "OrderwiseStockTransactions");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockTransactions_Unit", table: "OrderwiseStockTransactions");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockMasters_Currency", table: "OrderwiseStockMasters");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStockMasters_Unit", table: "OrderwiseStockMasters");
            migrationBuilder.DropIndex(name: "IX_OrderwiseStocks_Unit", table: "OrderwiseStocks");
            migrationBuilder.DropIndex(name: "IX_GarmentTypeItems_StockCode", table: "GarmentTypeItems");
            migrationBuilder.DropIndex(name: "IX_GarmentTypeItems_Unit", table: "GarmentTypeItems");
            migrationBuilder.DropIndex(name: "IX_GarmentAdditionalCosts_Currency", table: "GarmentAdditionalCosts");
            migrationBuilder.DropIndex(name: "IX_GarmentAdditionalCosts_Unit", table: "GarmentAdditionalCosts");
            migrationBuilder.DropIndex(name: "IX_Styles_Unit", table: "Styles");
            migrationBuilder.DropIndex(name: "IX_SubContracts_Currency", table: "SubContracts");
            migrationBuilder.DropIndex(name: "IX_SubContracts_Unit", table: "SubContracts");
            migrationBuilder.DropIndex(name: "IX_PartShipments_Unit", table: "PartShipments");
            migrationBuilder.DropIndex(name: "IX_QuotaTransactions_Unit", table: "QuotaTransactions");

            // Prerequisites last.
            migrationBuilder.DropIndex(name: "IX_Basis_Code", table: "Basis");

            // Reverse the Styles.Unit narrowing (must happen after FK_Styles_Units_Unit/IX_Styles_Unit
            // are already dropped above, and before Units.Code widens back below).
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "Styles",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            // Reverse the Banks.CurrencyCode / PurchaseOrders.UnitCode / PurchaseOrders.CurrencyCode
            // narrowing (same reasoning as Styles.Unit above).
            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "Banks",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "PurchaseOrders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "PurchaseOrders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            // ---- Reverse the Currencies.Code / Units.Code narrowing ----
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLines_Currencies_CurrencyCode",
                table: "ProductionLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLines_Units_UnitCode",
                table: "ProductionLines");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Units_Code",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "ProductionLines",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "ProductionLines",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Currencies",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Units",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Units_Code",
                table: "Units",
                column: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLines_Currencies_CurrencyCode",
                table: "ProductionLines",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLines_Units_UnitCode",
                table: "ProductionLines",
                column: "UnitCode",
                principalTable: "Units",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
