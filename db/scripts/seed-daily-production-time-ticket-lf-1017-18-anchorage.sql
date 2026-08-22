/* =====================================================================
   Daily Production Time Ticket data load - LF/1017-18/MJKTS/ANCHORAGE
   (PR_DPTT.DBF -> DailyProductionTimeTicketEntries)

   Source: C:\AppPro-Claude\prod\PR_DPTT.csv - 22 rows matched this style;
   4 excluded (empty OP_CODE, QTY=0 - legacy trailing blank-row artifacts
   from DBEDIT, not real entries). The 18 remaining rows are below.

   FLAG: 9 of these 18 rows use operations (COFR, RCFS, BAJO) that are NOT
   in this style's current StyleOperationBreakdowns (only CPLR, CUAL, TCNR,
   UCBR, UCPA, UCPR exist there right now). They insert cleanly - the FK is
   only to the Operations master, not the style's breakdown - but the
   Daily Production Time Ticket screen's efficiency calculator will show
   SAM = 0 / Earned Minutes = 0 for those specific rows until (if) those
   operations are added to ANCHORAGE's Operation Breakdown.

   All Employee/Operation/NonProductiveHourCode/ProductionLine references
   verified against your live tables before writing this.

   Idempotent: safe to re-run - only inserts rows whose full key doesn't
   already exist (checked: 0 existing rows for this style before this
   script was written).
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.DailyProductionTimeTicketEntries
    (Date, LineCode, BuyerCode, [Order], TypeCode, StyleCode, EmployeeCode, OperationCode,
     Quantity, NonProductiveHourCode, NonProductiveHours, WorkHours)
SELECT v.Date, v.LineCode, v.BuyerCode, v.[Order], v.TypeCode, v.StyleCode, v.EmployeeCode, v.OperationCode,
       v.Quantity, v.NonProductiveHourCode, v.NonProductiveHours, v.WorkHours
FROM (VALUES
    (CAST('1995-06-30' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0013', N'UCPR', 100, N'MT', 4.00, 7.50),
    (CAST('1995-06-30' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0038', N'UCBR', 200, NULL,  0.00, 7.50),
    (CAST('1995-07-11' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0494', N'COFR', 0,   NULL,  0.00, 0.00),
    (CAST('1995-10-11' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'0013', N'UCPR', 200, NULL,  0.00, 7.50),
    (CAST('1995-10-11' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'0184', N'UCBR', 100, N'CL', 1.00, 7.50),
    (CAST('1995-10-11' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'0662', N'UCPA', 34,  NULL,  0.00, 7.50),
    (CAST('1996-02-12' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'0013', N'COFR', 120, NULL,  0.00, 7.50),
    (CAST('1996-02-12' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'0086', N'CPLR', 65,  N'CL', 1.00, 7.50),
    (CAST('1996-02-12' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'0220', N'RCFS', 175, N'MT', 2.00, 7.50),
    (CAST('1996-02-12' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'2379', N'TCNR', 100, NULL,  0.00, 7.50),
    (CAST('1996-02-12' AS date), N'L-C', 1, N'1017-18', 2, N'ANCHORAGE', N'2443', N'UCPA', 200, NULL,  0.00, 3.25),
    (CAST('1996-02-24' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0086', N'BAJO', 150, NULL,  0.00, 4.00),
    (CAST('1996-02-24' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0116', N'COFR', 240, N'CL', 0.50, 4.00),
    (CAST('1996-02-24' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0572', N'CPLR', 173, NULL,  0.00, 7.50),
    (CAST('1996-02-23' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0581', N'COFR', 267, NULL,  0.00, 7.50),
    (CAST('1996-02-23' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0649', N'BAJO', 123, NULL,  0.00, 7.50),
    (CAST('1996-02-23' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'1012', N'BAJO', 43,  N'EH', 1.00, 7.50),
    (CAST('1996-02-27' AS date), N'L-A', 1, N'1017-18', 2, N'ANCHORAGE', N'0092', N'COFR', 120, NULL,  0.00, 7.50)
) AS v(Date, LineCode, BuyerCode, [Order], TypeCode, StyleCode, EmployeeCode, OperationCode,
       Quantity, NonProductiveHourCode, NonProductiveHours, WorkHours)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DailyProductionTimeTicketEntries e
    WHERE e.Date = v.Date AND e.LineCode = v.LineCode
      AND e.BuyerCode = v.BuyerCode AND e.[Order] = v.[Order]
      AND e.TypeCode = v.TypeCode AND e.StyleCode = v.StyleCode
      AND e.EmployeeCode = v.EmployeeCode AND e.OperationCode = v.OperationCode
);

SELECT * FROM dbo.DailyProductionTimeTicketEntries
WHERE BuyerCode = 1 AND [Order] = '1017-18' AND TypeCode = 2 AND StyleCode = 'ANCHORAGE'
ORDER BY Date, LineCode, EmployeeCode;
