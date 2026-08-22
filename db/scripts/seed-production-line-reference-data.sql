/* =====================================================================
   Production Control - ProductionLines reference data load
   (completes the 6-table Phase 1 reference set; the other 5 tables were
   loaded by seed-production-reference-data.sql, already run successfully)

   Source: C:\AppPro-Claude\prod\OD_LINE.csv (3 rows).

   Plain INSERT ... VALUES, same as the other script - no BULK INSERT,
   no special permission needed.

   NOTE - CurrencyCode translation: the CSV's CURR column holds the
   legacy symbol 'US$' for every row. I checked the live Currencies
   table directly (ProductionLines.CurrencyCode is FK'd to
   Currencies.Code) - it has ISO code 'USD', not 'US$', so inserting the
   raw CSV value would violate the foreign key. Mapped US$ -> USD below;
   flag this comment if that mapping is wrong.

   NOTE - date columns: NEXT_ALC / NEXT_ALC1 are legacy YYMMDD strings
   (e.g. 960907). Expanded to 19YY-MM-DD (assumes 1990s dates, matching
   this system's actual era). Blank NEXT_ALC1 values become NULL.

   Idempotent: safe to re-run - only inserts rows whose LineCode doesn't
   already exist.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- ---------------------------------------------------------------------
-- ProductionLines  (OD_LINE.csv, 3 rows)
-- ---------------------------------------------------------------------
INSERT INTO dbo.ProductionLines
    (LineCode, Description, NumberOfMachines, CurrencyCode, LineCostPerDay,
     MinimumProductionPerOrder, UnitCode, NextAllocationDate, EstimatedNextAllocationDate)
SELECT
    v.LineCode, v.Description, v.NumberOfMachines, v.CurrencyCode, v.LineCostPerDay,
    v.MinimumProductionPerOrder, v.UnitCode, v.NextAllocationDate, v.EstimatedNextAllocationDate
FROM (VALUES
  (N'L-A', N'LINE A', 65, 'USD', 800.000, 3000, 'PCS', CAST('1996-09-07' AS date), CAST('1996-03-17' AS date)),
  (N'L-B', N'LINE B', 65, 'USD', 800.000, 1500, 'PCS', CAST('1996-02-22' AS date), NULL),
  (N'L-C', N'LINE C', 50, 'USD', 750.000, 2000, 'PCS', CAST('1996-03-23' AS date), NULL)
) AS v(LineCode, Description, NumberOfMachines, CurrencyCode, LineCostPerDay,
       MinimumProductionPerOrder, UnitCode, NextAllocationDate, EstimatedNextAllocationDate)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ProductionLines p WHERE p.LineCode = v.LineCode
);

COMMIT TRANSACTION;

SELECT * FROM dbo.ProductionLines;
