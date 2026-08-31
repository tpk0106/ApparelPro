/*
    04_Import_GeneralPurchaseOrders.sql
    ------------------------------------
    Imports the legacy General Inventory Purchase Order line items (gi_podet.dbf,
    exported as GI_PODET.csv) into GeneralPurchaseOrderDetails, and synthesizes a
    minimal GeneralPurchaseOrders header row per distinct PO_NO to satisfy the FK
    (GeneralPurchaseOrderDetails.PoNumber -> GeneralPurchaseOrders.PoNumber).

    >>> IMPORTANT - GI_POHED.csv (the real PO header file: Supplier, Date, Basis,
    >>> PI No, Currency, UserId) has ZERO data rows in the export supplied - header
    >>> only, no data. There is therefore no real SupplierCode available for any P/O.
    >>>
    >>> Per explicit instruction, this script synthesizes a PLACEHOLDER header row
    >>> for every PO_NO found in GI_PODET.csv, with:
    >>>   SupplierCode = '000000'   (placeholder - not a real Supplier)
    >>>   OrderDate = a deterministic pseudo-random date spread across 01/01/1995 to
    >>>               30/12/1995 (PoNumber-seeded, so re-running this script gives the
    >>>               same date every time) - needed so date-range reports (Stock
    >>>               Status/Movement/List of P/O's) have something to filter against.
    >>>   OrderTime / BasisCode / ProformaInvoiceNo / ProformaInvoiceDate / CurrencyCode
    >>>   / UserId = NULL
    >>>
    >>> These placeholder headers MUST be corrected manually later (via the General
    >>> P/O Entry screen, or a follow-up UPDATE) once/if real GI_POHED data turns up.
    >>> The sanity-check query at the bottom of this script lists every placeholder
    >>> header still outstanding, so you can find them again.

    Source file column order (semicolon-delimited, NOT RFC4180 - see 01_/02_/03_
    scripts for why BULK INSERT with a plain field/row terminator is used):
        PO_NO;STORE_CD;ITEM_CD;REF_NO;UNIT;ORD_QTY;PRICE;EXP_DATE;BALANCE;CPU

    NOTE: same as GI_STTR.csv - the header row names a trailing CPU column, but data
    rows only have 9 fields (CPU absent, not blank). Staging table matches the real
    9-column data shape.

    EXP_DATE is legacy YYMMDD (e.g. "940420" = 1994-04-20), windowed the same way as
    the 03_ script: 00-49 -> 20xx, 50-99 -> 19xx.

    Idempotent: re-running this script will not create duplicate PO headers or
    duplicate detail lines.

    >>> Update the file paths below to wherever the CSVs are located on this machine,
    >>> then run this whole script in SSMS against the ApparelPro database. <<<
*/

USE ApparelPro;
GO

IF OBJECT_ID('tempdb..#GiPodetStaging') IS NOT NULL DROP TABLE #GiPodetStaging;

CREATE TABLE #GiPodetStaging
(
    PO_NO       VARCHAR(20)  NULL,
    STORE_CD    VARCHAR(20)  NULL,
    ITEM_CD     VARCHAR(50)  NULL,
    REF_NO      VARCHAR(50)  NULL,
    UNIT        VARCHAR(20)  NULL,
    ORD_QTY     VARCHAR(50)  NULL,
    PRICE       VARCHAR(50)  NULL,
    EXP_DATE    VARCHAR(20)  NULL,
    BALANCE     VARCHAR(50)  NULL
);

BULK INSERT #GiPodetStaging
FROM 'C:\AppPro-Claude\gi\GI_PODET.csv'   -- >>> update this path if needed <<<
WITH
(
    FIRSTROW = 2,
    FIELDTERMINATOR = ';',
    ROWTERMINATOR = '0x0a',
    CODEPAGE = 'ACP',
    TABLOCK
);

-- Drop any stray CR left over from CRLF line endings when ROWTERMINATOR only matched LF.
UPDATE #GiPodetStaging SET BALANCE = REPLACE(BALANCE, CHAR(13), '') WHERE BALANCE LIKE '%' + CHAR(13);

-- Step 1: one placeholder header row per distinct PO_NO (see warning block above).
INSERT INTO GeneralPurchaseOrders
(PoNumber, SupplierCode, OrderDate, OrderTime, BasisCode, ProformaInvoiceNo, ProformaInvoiceDate, CurrencyCode, UserId)
SELECT DISTINCT
    PoNumber     = LTRIM(RTRIM(s.PO_NO)),
    SupplierCode = '000000',
    OrderDate    = DATEADD(DAY, ABS(CHECKSUM(LTRIM(RTRIM(s.PO_NO)))) % 364, '19950101'),
    OrderTime    = DATEADD(SECOND, ABS(CHECKSUM(LTRIM(RTRIM(s.PO_NO)) + 'T')) % 86400, '00:00:00'),
    BasisCode    = NULL,
    ProformaInvoiceNo   = NULL,
    ProformaInvoiceDate = NULL,
    CurrencyCode = NULL,
    UserId       = NULL
FROM #GiPodetStaging s
WHERE NULLIF(LTRIM(RTRIM(s.PO_NO)), '') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM GeneralPurchaseOrders p WHERE p.PoNumber = LTRIM(RTRIM(s.PO_NO)));

-- Step 1b: backfill OrderDate/OrderTime on placeholder headers that were already
-- inserted by an earlier run of this script (before OrderDate/OrderTime were added) -
-- the INSERT above only reaches PO_NOs that don't exist yet, so already-imported rows
-- need this separate fix.
UPDATE p
SET p.OrderDate = ISNULL(p.OrderDate, DATEADD(DAY, ABS(CHECKSUM(p.PoNumber)) % 364, '19950101')),
    p.OrderTime = ISNULL(p.OrderTime, DATEADD(SECOND, ABS(CHECKSUM(p.PoNumber + 'T')) % 86400, '00:00:00'))
FROM GeneralPurchaseOrders p
WHERE p.SupplierCode = '000000' AND (p.OrderDate IS NULL OR p.OrderTime IS NULL);

-- Step 2: the detail lines themselves.
INSERT INTO GeneralPurchaseOrderDetails
(PoNumber, StoreCode, ItemCode, RefNo, Unit, OrderedQuantity, Price, ExpectedDate, Balance)
SELECT
    PoNumber        = LTRIM(RTRIM(s.PO_NO)),
    StoreCode       = LTRIM(RTRIM(s.STORE_CD)),
    ItemCode        = LTRIM(RTRIM(s.ITEM_CD)),
    RefNo           = NULLIF(LTRIM(RTRIM(s.REF_NO)), ''),
    Unit            = LTRIM(RTRIM(s.UNIT)),
    OrderedQuantity = ISNULL(TRY_CAST(s.ORD_QTY AS DECIMAL(12, 2)), 0),
    Price           = ISNULL(TRY_CAST(s.PRICE AS DECIMAL(10, 4)), 0),
    ExpectedDate    = CASE WHEN NULLIF(LTRIM(RTRIM(s.EXP_DATE)), '') IS NULL THEN NULL ELSE
                           DATEFROMPARTS(
                               CASE WHEN TRY_CAST(LEFT(s.EXP_DATE, 2) AS INT) <= 49
                                    THEN 2000 + TRY_CAST(LEFT(s.EXP_DATE, 2) AS INT)
                                    ELSE 1900 + TRY_CAST(LEFT(s.EXP_DATE, 2) AS INT)
                               END,
                               TRY_CAST(SUBSTRING(s.EXP_DATE, 3, 2) AS INT),
                               TRY_CAST(SUBSTRING(s.EXP_DATE, 5, 2) AS INT)
                           )
                      END,
    Balance         = ISNULL(TRY_CAST(s.BALANCE AS DECIMAL(12, 2)), 0)
FROM #GiPodetStaging s
WHERE
    NULLIF(LTRIM(RTRIM(s.PO_NO)), '') IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM GeneralPurchaseOrderDetails d
        WHERE d.PoNumber = LTRIM(RTRIM(s.PO_NO))
          AND d.StoreCode = LTRIM(RTRIM(s.STORE_CD))
          AND d.ItemCode = LTRIM(RTRIM(s.ITEM_CD))
          AND ISNULL(d.RefNo, '') = ISNULL(NULLIF(LTRIM(RTRIM(s.REF_NO)), ''), '')
          AND d.OrderedQuantity = ISNULL(TRY_CAST(s.ORD_QTY AS DECIMAL(12, 2)), 0)
          AND d.Price = ISNULL(TRY_CAST(s.PRICE AS DECIMAL(10, 4)), 0)
    );

-- Sanity checks.
SELECT ImportedHeaderCount = COUNT(*) FROM GeneralPurchaseOrders WHERE SupplierCode = '000000';
SELECT ImportedDetailCount = COUNT(*) FROM GeneralPurchaseOrderDetails;
SELECT StagingRowCount = COUNT(*) FROM #GiPodetStaging;

-- Every placeholder header still outstanding - fix these up once real supplier/date
-- data is available (or use the General P/O Entry screen to edit each one).
SELECT PoNumber FROM GeneralPurchaseOrders WHERE SupplierCode = '000000' ORDER BY PoNumber;

DROP TABLE #GiPodetStaging;
