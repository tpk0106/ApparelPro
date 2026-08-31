/*
    03_Import_GeneralStockTransactions.sql
    ---------------------------------------
    Imports the legacy General Inventory transaction ledger (gi_sttr.dbf, exported as
    GI_STTR.csv) into GeneralStockTransactions.

    Source file column order (semicolon-delimited, NOT RFC4180 - fields are never
    quote-escaped, so BULK INSERT with a plain field/row terminator is used, same as
    the 01_/02_ scripts):
        ID;DOCNO;DATE;TIME;INV_NO;STORE_CD;ITEM_CD;UNIT;QTY;SUPP_CD;PRICE;CURR;EX_RATE;BUYER;ORDER;PO_NO

    NOTE: the header row names a trailing CPU column, but every data row actually has
    only 16 fields (confirmed across all 1510 rows) - CPU is entirely absent, not just
    blank. The staging table below is 16 columns, ending at PO_NO, to match the real data.

    Notes:
      - DATE is legacy YYMMDD (e.g. "940301" = 1994-03-01). Two-digit year is windowed
        00-49 -> 20xx, 50-99 -> 19xx.
      - ItemCode here is already the trimmed 22-char-max composite code (same format as
        GI_STMST.csv/GeneralStockMasters.ItemCode, not the pre-split segments GI_STREF.csv
        used) - no reconstruction needed, just trim.
      - The legacy BUYER column is overloaded: on GRN/GIN rows it is a real buyer tie-back,
        but GI_STRN1.PRG reuses it as the "To Department" code on SRN-type rows, and
        GI_GIN1.PRG reuses PO_NO on an SRN row as a linked GIN number. This raw historical
        ledger dump has no reliable way to tell those cases apart row-by-row, so BUYER is
        loaded only into BuyerCode as-is; DepartmentCode and LinkedDocumentNumber are left
        NULL for every imported row. None of the current reports (including Stock Status)
        read those two columns, so this does not affect report output.
      - Idempotent: re-running this script will not create duplicate rows, keyed on
        (TransactionTypeCode, DocumentNumber, StoreCode, ItemCode, TransactionDate, TransactionTime).

    >>> Update the file path below to wherever GI_STTR.csv is located on this machine,
    >>> then run this whole script in SSMS against the ApparelPro database. <<<
*/

USE ApparelPro;
GO

IF OBJECT_ID('tempdb..#GiSttrStaging') IS NOT NULL DROP TABLE #GiSttrStaging;

CREATE TABLE #GiSttrStaging
(
    ID          VARCHAR(10)  NULL,
    DOCNO       VARCHAR(20)  NULL,
    [DATE]      VARCHAR(20)  NULL,
    [TIME]      VARCHAR(20)  NULL,
    INV_NO      VARCHAR(50)  NULL,
    STORE_CD    VARCHAR(20)  NULL,
    ITEM_CD     VARCHAR(50)  NULL,
    UNIT        VARCHAR(20)  NULL,
    QTY         VARCHAR(50)  NULL,
    SUPP_CD     VARCHAR(20)  NULL,
    PRICE       VARCHAR(50)  NULL,
    CURR        VARCHAR(20)  NULL,
    EX_RATE     VARCHAR(50)  NULL,
    BUYER       VARCHAR(50)  NULL,
    [ORDER]     VARCHAR(50)  NULL,
    PO_NO       VARCHAR(50)  NULL
);

BULK INSERT #GiSttrStaging
FROM 'C:\AppPro-Claude\gi\GI_STTR.csv'   -- >>> update this path if needed <<<
WITH
(
    FIRSTROW = 2,
    FIELDTERMINATOR = ';',
    ROWTERMINATOR = '0x0a',
    CODEPAGE = 'ACP',
    TABLOCK
);

-- Drop any stray CR left over from CRLF line endings when ROWTERMINATOR only matched LF.
UPDATE #GiSttrStaging SET PO_NO = REPLACE(PO_NO, CHAR(13), '') WHERE PO_NO LIKE '%' + CHAR(13);

INSERT INTO GeneralStockTransactions
(
    TransactionTypeCode, DocumentNumber, TransactionDate, TransactionTime,
    InvoiceNumber, StoreCode, ItemCode, Unit, Quantity,
    SupplierCode, Price, Currency, ExchangeRate,
    BuyerCode, [Order], PoNumber, DepartmentCode, LinkedDocumentNumber
)
SELECT
    TransactionTypeCode = LTRIM(RTRIM(s.ID)),
    DocumentNumber      = LTRIM(RTRIM(s.DOCNO)),
    TransactionDate     = DATEFROMPARTS(
                               CASE WHEN TRY_CAST(LEFT(s.[DATE], 2) AS INT) <= 49
                                    THEN 2000 + TRY_CAST(LEFT(s.[DATE], 2) AS INT)
                                    ELSE 1900 + TRY_CAST(LEFT(s.[DATE], 2) AS INT)
                               END,
                               TRY_CAST(SUBSTRING(s.[DATE], 3, 2) AS INT),
                               TRY_CAST(SUBSTRING(s.[DATE], 5, 2) AS INT)
                           ),
    TransactionTime     = TRY_CONVERT(TIME, NULLIF(LTRIM(RTRIM(s.[TIME])), ''), 108),
    InvoiceNumber        = NULLIF(LTRIM(RTRIM(s.INV_NO)), ''),
    StoreCode             = LTRIM(RTRIM(s.STORE_CD)),
    ItemCode              = LTRIM(RTRIM(s.ITEM_CD)),
    Unit                  = LTRIM(RTRIM(s.UNIT)),
    Quantity              = ISNULL(TRY_CAST(s.QTY AS DECIMAL(12, 2)), 0),
    SupplierCode          = NULLIF(LTRIM(RTRIM(s.SUPP_CD)), ''),
    Price                 = ISNULL(TRY_CAST(s.PRICE AS DECIMAL(12, 4)), 0),
    Currency              = NULLIF(LTRIM(RTRIM(s.CURR)), ''),
    ExchangeRate          = TRY_CAST(s.EX_RATE AS DECIMAL(7, 3)),
    BuyerCode             = TRY_CAST(NULLIF(LTRIM(RTRIM(s.BUYER)), '') AS INT),
    [Order]               = NULLIF(LTRIM(RTRIM(s.[ORDER])), ''),
    PoNumber              = NULLIF(LTRIM(RTRIM(s.PO_NO)), ''),
    DepartmentCode        = NULL,
    LinkedDocumentNumber  = NULL
FROM #GiSttrStaging s
WHERE
    NULLIF(LTRIM(RTRIM(s.ID)), '') IS NOT NULL
    AND NULLIF(LTRIM(RTRIM(s.[DATE])), '') IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM GeneralStockTransactions t
        WHERE t.TransactionTypeCode = LTRIM(RTRIM(s.ID))
          AND t.DocumentNumber      = LTRIM(RTRIM(s.DOCNO))
          AND t.StoreCode           = LTRIM(RTRIM(s.STORE_CD))
          AND t.ItemCode            = LTRIM(RTRIM(s.ITEM_CD))
          AND t.TransactionDate     = DATEFROMPARTS(
                                           CASE WHEN TRY_CAST(LEFT(s.[DATE], 2) AS INT) <= 49
                                                THEN 2000 + TRY_CAST(LEFT(s.[DATE], 2) AS INT)
                                                ELSE 1900 + TRY_CAST(LEFT(s.[DATE], 2) AS INT)
                                           END,
                                           TRY_CAST(SUBSTRING(s.[DATE], 3, 2) AS INT),
                                           TRY_CAST(SUBSTRING(s.[DATE], 5, 2) AS INT)
                                       )
          AND (
                t.TransactionTime = TRY_CONVERT(TIME, NULLIF(LTRIM(RTRIM(s.[TIME])), ''), 108)
                OR (t.TransactionTime IS NULL AND NULLIF(LTRIM(RTRIM(s.[TIME])), '') IS NULL)
              )
    );

-- Sanity check: rows imported, plus any rows that failed to parse a date (would show up as NULL TransactionDate).
SELECT ImportedRowCount = COUNT(*) FROM GeneralStockTransactions;
SELECT StagingRowCount = COUNT(*) FROM #GiSttrStaging;
SELECT UnparsedDateCount = COUNT(*) FROM #GiSttrStaging s
WHERE TRY_CAST(LEFT(s.[DATE], 2) AS INT) IS NULL
   OR TRY_CAST(SUBSTRING(s.[DATE], 3, 2) AS INT) IS NULL
   OR TRY_CAST(SUBSTRING(s.[DATE], 5, 2) AS INT) IS NULL;

DROP TABLE #GiSttrStaging;
