/* =====================================================================
   Style Component Breakdown data load - one style
   (PR_OPD1.DBF -> StyleComponentBreakdowns)

   Source row in C:\AppPro-Claude\prod\PR_OPD1.csv (lines 52-56):
       LF;1017-18;MJKTS;ANCHORAGE;1;COLL
       LF;1017-18;MJKTS;ANCHORAGE;2;CUFF
       LF;1017-18;MJKTS;ANCHORAGE;3;OUSH
       LF;1017-18;MJKTS;ANCHORAGE;4;FISH
       LF;1017-18;MJKTS;ANCHORAGE;5;W/BA

   Legacy codes resolved against your live Styles/Buyers/GarmentTypes data
   (not guessed): Buyer 'LF' -> Buyers.BuyerCode = 1 (LONDON FOG INDUSTRIES
   INC.), Type 'MJKTS' -> GarmentTypes.Id = 2 (Mens Jacket) - confirmed via
   the actual Styles row for Order '1017-18' / Style 'ANCHORAGE', not
   inferred from the abbreviation alone. Note the requested Type was typed
   as "MJKT" but the real legacy/DB value is "MJKTS" (5 chars) - flagging
   in case that wasn't intentional.

   All 5 ComponentCodes verified to already exist in GarmentComponents.
   Idempotent: safe to re-run - only inserts rows whose key doesn't
   already exist (checked: 0 existing rows for this style before this
   script was written).
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.StyleComponentBreakdowns
    (BuyerCode, [Order], TypeCode, StyleCode, ComponentSequence, ComponentCode)
SELECT v.BuyerCode, v.[Order], v.TypeCode, v.StyleCode, v.ComponentSequence, v.ComponentCode
FROM (VALUES
    (1, N'1017-18', 2, N'ANCHORAGE', 1, N'COLL'),
    (1, N'1017-18', 2, N'ANCHORAGE', 2, N'CUFF'),
    (1, N'1017-18', 2, N'ANCHORAGE', 3, N'OUSH'),
    (1, N'1017-18', 2, N'ANCHORAGE', 4, N'FISH'),
    (1, N'1017-18', 2, N'ANCHORAGE', 5, N'W/BA')
) AS v(BuyerCode, [Order], TypeCode, StyleCode, ComponentSequence, ComponentCode)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.StyleComponentBreakdowns s
    WHERE s.BuyerCode = v.BuyerCode AND s.[Order] = v.[Order]
      AND s.TypeCode = v.TypeCode AND s.StyleCode = v.StyleCode
      AND s.ComponentSequence = v.ComponentSequence
);

SELECT * FROM dbo.StyleComponentBreakdowns
WHERE BuyerCode = 1 AND [Order] = '1017-18' AND TypeCode = 2 AND StyleCode = 'ANCHORAGE'
ORDER BY ComponentSequence;
