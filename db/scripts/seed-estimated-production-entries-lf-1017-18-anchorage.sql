/* =====================================================================
   Estimated Production Entry data load - LF/1017-18/MJKTS/ANCHORAGE
   (PR_ESTD.DBF -> EstimatedProductionEntries)

   Source: C:\AppPro-Claude\prod\PR_ESTD.csv - all 15 rows matching this
   style, across 2 lines (L-A: 11 rows, L-C: 4 rows). No blank/placeholder
   rows found this time (unlike PR_DPTT) - every row has a real date and
   quantity.

   FLAG: only Line L-A currently has a ProductionLineAllocation row for
   this style (from earlier Phase 3 testing) - L-C does not. This script
   inserts both lines' historical data fine (no DB-level FK from this
   table to ProductionLineAllocations, only to ProductionLines itself),
   but if you later edit and re-save the L-C rows through the actual
   Estimated Production Entry screen, the backend will reject the save
   with "Line not allocated for this Buyer/Order/Type/Style" until L-C
   is allocated too (via the Production Line Allocation screen).

   Idempotent: safe to re-run - only inserts rows whose full key doesn't
   already exist (checked: 0 existing rows for this style before this
   script was written).
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.EstimatedProductionEntries
    (BuyerCode, [Order], TypeCode, StyleCode, LineCode, Date, Unit, Quantity)
SELECT v.BuyerCode, v.[Order], v.TypeCode, v.StyleCode, v.LineCode, v.Date, v.Unit, v.Quantity
FROM (VALUES
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-C', CAST('1994-11-29' AS date), N'PCS', 50.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-C', CAST('1994-11-30' AS date), N'PCS', 75.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-C', CAST('1994-12-01' AS date), N'PCS', 45.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-C', CAST('1994-12-02' AS date), N'PCS', 34.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-01' AS date), N'PCS', 330.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-02' AS date), N'PCS', 45.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-03' AS date), N'PCS', 335.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-04' AS date), N'PCS', 50.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-05' AS date), N'PCS', 54.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-06' AS date), N'PCS', 454.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-07' AS date), N'PCS', 54.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-08' AS date), N'PCS', 45.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-09' AS date), N'PCS', 45.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-10' AS date), N'PCS', 454.0),
    (1, N'1017-18', 2, N'ANCHORAGE', N'L-A', CAST('1995-04-11' AS date), N'PCS', 454.0)
) AS v(BuyerCode, [Order], TypeCode, StyleCode, LineCode, Date, Unit, Quantity)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EstimatedProductionEntries e
    WHERE e.BuyerCode = v.BuyerCode AND e.[Order] = v.[Order]
      AND e.TypeCode = v.TypeCode AND e.StyleCode = v.StyleCode
      AND e.LineCode = v.LineCode AND e.Date = v.Date
);

SELECT * FROM dbo.EstimatedProductionEntries
WHERE BuyerCode = 1 AND [Order] = '1017-18' AND TypeCode = 2 AND StyleCode = 'ANCHORAGE'
ORDER BY LineCode, Date;
