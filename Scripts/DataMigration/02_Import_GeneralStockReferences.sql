/*
    Imports legacy gi_stref.dbf data (exported as GI_STREF.csv) into
    GeneralStockReferences (item descriptions). Run this in SSMS against the
    ApparelPro database, after 01_Import_GeneralStockMasters.sql.

    Source file: C:\AppPro-Claude\gi\GI_STREF.csv
    Columns:     STOCK_CD;ITEM_CD;FEAT1;FEAT2;FEAT3;FEAT4;DESC;CPU
    Note: semicolon-delimited, NOT RFC4180 CSV - fields are not quote-escaped (a literal
    " inside a feature value, e.g. 1/2", is just data). BULK INSERT is used precisely
    because it does plain field/row-terminator splitting and never interprets " as a
    quote character.

    Unlike GI_STMST.csv (which stores the full 22-char composite ItemCode as one
    already-trimmed field), GI_STREF.csv stores the same composite pre-split into its 6
    fixed-width segments: StockCode(2) + ItemCode(4) + Feature1-4(4 each). This script
    reconstructs the composite by right-padding each segment to its column width with
    spaces, concatenating, then trimming only the *trailing* whitespace of the final
    22-char string (RTRIM - never trimming each segment individually first). That
    exactly reproduces GI_STMST's own trimming: e.g. StockCode='03', ItemCode='11GT',
    Feature1='TAN', Feature2='2"', Feature3='41MT' composes to '0311GTTAN 2"  41MT',
    matching GeneralStockMasters.ItemCode for the same item after
    01_Import_GeneralStockMasters.sql. Getting this wrong would silently break every
    description lookup joined on ItemCode across the app - verify a handful of rows
    against GeneralStockMasters after running (query at the bottom of this script).

    Upserts by composed ItemCode: existing rows get their Description updated, new
    ones are inserted. Safe to re-run after the source CSV changes. CPU has no
    equivalent column and is ignored.
*/

SET NOCOUNT ON;

-- ---------------------------------------------------------------------------
-- 1. Stage the raw CSV (all text - safest for BULK INSERT).
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#GiStrefStaging') IS NOT NULL DROP TABLE #GiStrefStaging;

CREATE TABLE #GiStrefStaging (
    StockCd NVARCHAR(50) NULL,
    ItemCd  NVARCHAR(50) NULL,
    Feat1   NVARCHAR(50) NULL,
    Feat2   NVARCHAR(50) NULL,
    Feat3   NVARCHAR(50) NULL,
    Feat4   NVARCHAR(50) NULL,
    Descr   NVARCHAR(200) NULL,
    Cpu     NVARCHAR(50) NULL
);

BULK INSERT #GiStrefStaging
FROM 'C:\AppPro-Claude\gi\GI_STREF.csv'
WITH (
    FIRSTROW = 2,
    FIELDTERMINATOR = ';',
    ROWTERMINATOR = '\n',
    CODEPAGE = '1252',
    KEEPNULLS
);

-- Defensively strip a stray trailing CR (CRLF line endings but a LF row
-- terminator leaves \r on the last column of every row).
UPDATE #GiStrefStaging SET Cpu = REPLACE(Cpu, CHAR(13), '');

-- ---------------------------------------------------------------------------
-- 2. Compose the 22-char composite ItemCode exactly as GI_STMST.csv stores it
--    (see header comment) and stage it alongside the trimmed Description.
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#GiStrefComposed') IS NOT NULL DROP TABLE #GiStrefComposed;

SELECT
    RTRIM(
        LEFT(LTRIM(RTRIM(StockCd)) + REPLICATE(' ', 2), 2) +
        LEFT(LTRIM(RTRIM(ItemCd))  + REPLICATE(' ', 4), 4) +
        LEFT(LTRIM(RTRIM(Feat1))   + REPLICATE(' ', 4), 4) +
        LEFT(LTRIM(RTRIM(Feat2))   + REPLICATE(' ', 4), 4) +
        LEFT(LTRIM(RTRIM(Feat3))   + REPLICATE(' ', 4), 4) +
        LEFT(LTRIM(RTRIM(Feat4))   + REPLICATE(' ', 4), 4)
    ) AS ItemCode,
    LTRIM(RTRIM(Descr)) AS Description
INTO #GiStrefComposed
FROM #GiStrefStaging
WHERE LTRIM(RTRIM(StockCd)) <> '' AND LTRIM(RTRIM(ItemCd)) <> '';

-- ---------------------------------------------------------------------------
-- 3. Upsert into GeneralStockReferences.
-- ---------------------------------------------------------------------------
UPDATE r
SET r.Description = c.Description
FROM GeneralStockReferences r
JOIN #GiStrefComposed c ON c.ItemCode = r.ItemCode;

PRINT CONCAT('GeneralStockReferences rows updated: ', @@ROWCOUNT);

INSERT INTO GeneralStockReferences (ItemCode, Description)
SELECT c.ItemCode, c.Description
FROM #GiStrefComposed c
WHERE NOT EXISTS (
    SELECT 1 FROM GeneralStockReferences r WHERE r.ItemCode = c.ItemCode
);

PRINT CONCAT('GeneralStockReferences rows inserted: ', @@ROWCOUNT);

DROP TABLE #GiStrefStaging;
DROP TABLE #GiStrefComposed;

-- ---------------------------------------------------------------------------
-- 4. Sanity check - items in GeneralStockMasters with no matching description
--    (join miss here means the composite-key reconstruction above didn't match
--    for that row - inspect before trusting the import).
-- ---------------------------------------------------------------------------
SELECT m.StoreCode, m.ItemCode
FROM GeneralStockMasters m
LEFT JOIN GeneralStockReferences r ON r.ItemCode = m.ItemCode
WHERE r.ItemCode IS NULL;
