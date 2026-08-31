/*
    Imports legacy gi_stmst.dbf data (exported as GI_STMST.csv) into GeneralStockMasters.
    Run this in SSMS against the ApparelPro database.

    Source file: C:\AppPro-Claude\gi\GI_STMST.csv
    Columns:     STORE_CD;ITEM_CD;UNIT;QTY_IN_HD;SHDW_BAL;VALUE;CURR;DAM_QTY;RO_LEVEL;
                 RO_QTY;MIN_STOCK;MAX_STOCK;CPU
    Note: semicolon-delimited, NOT RFC4180 CSV - fields are not quote-escaped (a literal
    " inside an item code, e.g. 1/2", is just data). BULK INSERT is used precisely
    because it does plain field/row-terminator splitting and never interprets " as a
    quote character, unlike most CSV-aware tools.

    GeneralStockMasters has FK constraints to Units.Code and Currencies.Code, so two
    legacy currency codes with no modern equivalent are mapped per an explicit decision
    (2026-08-28): US$ -> USD, SLR -> LKR. Two legacy unit codes (BAR, MTS) have no
    modern equivalent at all and are inserted into Units verbatim.

    Idempotent: rows already present for a given (StoreCode, ItemCode) pair (the
    table's real unique index, IX_GeneralStockMasters_StoreCode_ItemCode) are skipped
    via NOT EXISTS, so re-running after a partial run is safe. CPU has no equivalent
    column on GeneralStockMasters and is ignored.
*/

SET NOCOUNT ON;

-- ---------------------------------------------------------------------------
-- 1. Ensure reference data the FK constraints require is in place.
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Currencies WHERE Code = 'LKR')
BEGIN
    INSERT INTO Currencies (Code, Name, CountryCode, Minor)
    VALUES ('LKR', 'Sri Lanka Rupee', 'LKA', '');
    PRINT 'Inserted Currency LKR (Sri Lanka Rupee).';
END

IF NOT EXISTS (SELECT 1 FROM Units WHERE Code = 'BAR')
BEGIN
    INSERT INTO Units (Code, Description) VALUES ('BAR', 'BAR');
    PRINT 'Inserted Unit BAR.';
END

IF NOT EXISTS (SELECT 1 FROM Units WHERE Code = 'MTS')
BEGIN
    INSERT INTO Units (Code, Description) VALUES ('MTS', 'METERS');
    PRINT 'Inserted Unit MTS.';
END

-- ---------------------------------------------------------------------------
-- 2. Stage the raw CSV (all text - safest for BULK INSERT, converted below).
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#GiStmstStaging') IS NOT NULL DROP TABLE #GiStmstStaging;

CREATE TABLE #GiStmstStaging (
    StoreCd     NVARCHAR(50) NULL,
    ItemCd      NVARCHAR(50) NULL,
    Unit        NVARCHAR(50) NULL,
    QtyInHd     NVARCHAR(50) NULL,
    ShdwBal     NVARCHAR(50) NULL,
    Value       NVARCHAR(50) NULL,
    Curr        NVARCHAR(50) NULL,
    DamQty      NVARCHAR(50) NULL,
    RoLevel     NVARCHAR(50) NULL,
    RoQty       NVARCHAR(50) NULL,
    MinStock    NVARCHAR(50) NULL,
    MaxStock    NVARCHAR(50) NULL,
    Cpu         NVARCHAR(50) NULL
);

BULK INSERT #GiStmstStaging
FROM 'C:\AppPro-Claude\gi\GI_STMST.csv'
WITH (
    FIRSTROW = 2,
    FIELDTERMINATOR = ';',
    ROWTERMINATOR = '\n',
    CODEPAGE = '1252',
    KEEPNULLS
);

-- Defensively strip a stray trailing CR (CRLF line endings but a LF row
-- terminator leaves \r on the last column of every row).
UPDATE #GiStmstStaging SET Cpu = REPLACE(Cpu, CHAR(13), '');

-- ---------------------------------------------------------------------------
-- 3. Insert into GeneralStockMasters - blank numerics become 0 (NOT NULL
--    columns), currency codes mapped, duplicates skipped.
-- ---------------------------------------------------------------------------
INSERT INTO GeneralStockMasters
    (StoreCode, ItemCode, Unit, QtyInHand, ShadowBalance, Value, Currency,
     DamagedQuantity, ReorderLevel, ReorderQuantity, MinStock, MaxStock)
SELECT
    LTRIM(RTRIM(s.StoreCd)),
    LTRIM(RTRIM(s.ItemCd)),
    LTRIM(RTRIM(s.Unit)),
    TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.QtyInHd)), '')),
    TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.ShdwBal)), '')),
    TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.Value)), '')),
    CASE LTRIM(RTRIM(s.Curr))
        WHEN 'US$' THEN 'USD'
        WHEN 'SLR' THEN 'LKR'
        ELSE LTRIM(RTRIM(s.Curr))
    END,
    ISNULL(TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.DamQty)), '')), 0),
    ISNULL(TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.RoLevel)), '')), 0),
    ISNULL(TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.RoQty)), '')), 0),
    ISNULL(TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.MinStock)), '')), 0),
    ISNULL(TRY_CONVERT(DECIMAL(18,4), NULLIF(LTRIM(RTRIM(s.MaxStock)), '')), 0)
FROM #GiStmstStaging s
WHERE LTRIM(RTRIM(s.StoreCd)) <> ''
  AND LTRIM(RTRIM(s.ItemCd)) <> ''
  AND NOT EXISTS (
        SELECT 1 FROM GeneralStockMasters m
        WHERE m.StoreCode = LTRIM(RTRIM(s.StoreCd))
          AND m.ItemCode = LTRIM(RTRIM(s.ItemCd))
      );

PRINT CONCAT('GeneralStockMasters rows inserted: ', @@ROWCOUNT);

DROP TABLE #GiStmstStaging;
