/* =====================================================================
   Actual Production Entry data load - LF/1017-18/MJKTS/ANCHORAGE
   (PR_DPROD.DBF -> DailyProductionEntries)

   Source: C:\AppPro-Claude\prod\PR_DPROD.csv - all 48 rows matching this
   style, across 2 lines and all 6 sections (Cutting/Sewing/Checking/
   Finishing/Packing/Shipping): L-A on 1995-04-17 (6 rows) and L-C across
   6 dates in Dec 1995 (42 rows). Only the raw daily QTY column is
   loaded - the legacy TO_DT_QTY (running total) column is intentionally
   dropped, since this schema computes to-date quantity on read via
   SUM(Quantity), not a stored/cascaded column (see DailyProductionEntry.cs).

   FLAG: only Line L-A currently has a ProductionLineAllocation row for
   this style (from earlier Phase 3 testing, and it's dated 2026-08-22 to
   2026-09-04 - unrelated to this 1995 legacy data). L-C has no
   allocation at all. This script inserts both lines' historical data
   fine (DailyProductionEntries only has a DB-level FK to ProductionLines
   itself, not to ProductionLineAllocations), but if you later edit and
   re-save either line's rows through the actual Actual Production Entry
   screen, the backend will reject the save with "Line ... is not
   allocated for this Buyer/Order/Type/Style" until that line is
   allocated (via the Production Line Allocation screen).

   Idempotent: safe to re-run - only inserts rows whose full key doesn't
   already exist (checked: 0 existing rows for this style before this
   script was written).
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.DailyProductionEntries
    (Date, BuyerCode, [Order], TypeCode, StyleCode, LineCode, SectionCode, Hours, Unit, Quantity)
SELECT v.Date, v.BuyerCode, v.[Order], v.TypeCode, v.StyleCode, v.LineCode, v.SectionCode, v.Hours, v.Unit, v.Quantity
FROM (VALUES
    (CAST('1995-04-17' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-A', N'001', 4.0, N'PCS', 0.0),
    (CAST('1995-04-17' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-A', N'002', 4.0, N'PCS', 222.0),
    (CAST('1995-04-17' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-A', N'003', 4.0, N'PCS', 0.0),
    (CAST('1995-04-17' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-A', N'004', 4.0, N'PCS', 12.0),
    (CAST('1995-04-17' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-A', N'005', 4.0, N'PCS', 0.0),
    (CAST('1995-04-17' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-A', N'006', 4.0, N'PCS', 12.0),

    (CAST('1995-12-10' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 2.0, N'PCS', 150.0),
    (CAST('1995-12-10' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 2.0, N'PCS', 150.0),
    (CAST('1995-12-10' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 2.0, N'PCS', 150.0),
    (CAST('1995-12-10' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 2.0, N'PCS', 150.0),
    (CAST('1995-12-10' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 2.0, N'PCS', 150.0),
    (CAST('1995-12-10' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 2.0, N'PCS', 150.0),

    (CAST('1995-12-11' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 8.0, N'PCS', 172.0),
    (CAST('1995-12-11' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 8.0, N'PCS', 172.0),
    (CAST('1995-12-11' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 8.0, N'PCS', 0.0),
    (CAST('1995-12-11' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 8.0, N'PCS', 0.0),
    (CAST('1995-12-11' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 8.0, N'PCS', 0.0),
    (CAST('1995-12-11' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 8.0, N'PCS', 0.0),

    (CAST('1995-12-13' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 2.0, N'PCS', 25.0),
    (CAST('1995-12-13' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 2.0, N'PCS', 0.0),
    (CAST('1995-12-13' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 2.0, N'PCS', 0.0),
    (CAST('1995-12-13' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 2.0, N'PCS', 0.0),
    (CAST('1995-12-13' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 2.0, N'PCS', 0.0),
    (CAST('1995-12-13' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 2.0, N'PCS', 0.0),

    (CAST('1995-12-16' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 8.0, N'PCS', 500.0),
    (CAST('1995-12-16' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 8.0, N'PCS', 250.0),
    (CAST('1995-12-16' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 8.0, N'PCS', 250.0),
    (CAST('1995-12-16' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 8.0, N'PCS', 240.0),
    (CAST('1995-12-16' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 8.0, N'PCS', 200.0),
    (CAST('1995-12-16' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 8.0, N'PCS', 200.0),

    (CAST('1995-12-19' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 2.0, N'PCS', 12.0),
    (CAST('1995-12-19' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 2.0, N'PCS', 11.0),
    (CAST('1995-12-19' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 2.0, N'PCS', 10.0),
    (CAST('1995-12-19' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 2.0, N'PCS', 9.0),
    (CAST('1995-12-19' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 2.0, N'PCS', 8.0),
    (CAST('1995-12-19' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 2.0, N'PCS', 7.0),

    (CAST('1995-12-20' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 2.0, N'PCS', 200.0),
    (CAST('1995-12-20' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 2.0, N'PCS', 250.0),
    (CAST('1995-12-20' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 2.0, N'PCS', 0.0),
    (CAST('1995-12-20' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 2.0, N'PCS', 0.0),
    (CAST('1995-12-20' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 2.0, N'PCS', 0.0),
    (CAST('1995-12-20' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 2.0, N'PCS', 0.0),

    (CAST('1995-12-22' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'001', 2.0, N'PCS', 600.0),
    (CAST('1995-12-22' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'002', 2.0, N'PCS', 540.0),
    (CAST('1995-12-22' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'003', 2.0, N'PCS', 500.0),
    (CAST('1995-12-22' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'004', 2.0, N'PCS', 301.0),
    (CAST('1995-12-22' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'005', 2.0, N'PCS', 276.0),
    (CAST('1995-12-22' AS date), 1, N'1017-18', 2, N'ANCHORAGE', N'L-C', N'006', 2.0, N'PCS', 155.0)
) AS v(Date, BuyerCode, [Order], TypeCode, StyleCode, LineCode, SectionCode, Hours, Unit, Quantity)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DailyProductionEntries e
    WHERE e.Date = v.Date AND e.BuyerCode = v.BuyerCode AND e.[Order] = v.[Order]
      AND e.TypeCode = v.TypeCode AND e.StyleCode = v.StyleCode
      AND e.LineCode = v.LineCode AND e.SectionCode = v.SectionCode
);

SELECT * FROM dbo.DailyProductionEntries
WHERE BuyerCode = 1 AND [Order] = '1017-18' AND TypeCode = 2 AND StyleCode = 'ANCHORAGE'
ORDER BY LineCode, Date, SectionCode;
