/* =====================================================================
   Component/Operation Template data load
   (PR_MOP2.DBF -> ComponentOperationTemplates)

   Source: C:\AppPro-Claude\prod\PR_MOP2.csv, 56 rows total. 3 rows
   excluded (ComponentCode 1111/2222/3333) - test/junk data, don't exist
   in GarmentComponents and would violate the FK. All 53 remaining rows
   verified against your live Operations/GarmentComponents/MachineTypes
   tables - every OperationCode and MachineTypeCode already exists.

   This is why the Style Operation Breakdown accordion showed empty
   tables: seed-from-template had nothing to copy from since this table
   was never populated. Once this runs, opening a component's accordion
   for the first time on a style will auto-seed from these rows (matching
   PR_OPD2.PRG's original auto-copy behaviour).

   Idempotent: safe to re-run - only inserts rows whose
   (ComponentCode, OperationSequence) key doesn't already exist.
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.ComponentOperationTemplates
    (ComponentCode, OperationSequence, OperationCode, MachineTypeCode, Sam, NumberOfMachines)
SELECT v.ComponentCode, v.OperationSequence, v.OperationCode, v.MachineTypeCode, v.Sam, v.NumberOfMachines
FROM (VALUES
    (N'COLL', 1, N'UCPR', N'SN', 0.75, 0),
    (N'COLL', 2, N'UCBR', N'SN', 0.75, 0),
    (N'COLL', 3, N'UCPA', N'SN', 1.2, 0),
    (N'COLL', 4, N'TCNR', N'SN', 0.75, 0),
    (N'COLL', 5, N'CPLR', N'SN', 0.75, 0),
    (N'COLL', 6, N'TCNR', N'BA', 0, 0),
    (N'COLL', 7, N'CUAL', N'BA', 1.2, 0),
    (N'CUFF', 1, N'RCFS', N'SN', 1, 0),
    (N'CUFF', 2, N'RICJ', N'SN', 1.3, 0),
    (N'FISH', 1, N'WBAS', N'SN', 1.5, 0),
    (N'FISH', 2, N'WBAL', N'SN', 1.5, 0),
    (N'FISH', 3, N'COAL', N'SN', 1.33, 0),
    (N'FISH', 4, N'COAS', N'SN', 1.33, 0),
    (N'FISH', 5, N'SHZA', N'SN', 2, 0),
    (N'FISH', 6, N'SHLA', N'SN', 2, 0),
    (N'FISH', 7, N'CUAL', N'SN', 1.33, 0),
    (N'FISH', 8, N'CUAL', N'SN', 1.33, 0),
    (N'FISH', 9, N'CUFO', N'OL', 0.6, 0),
    (N'FISH', 10, N'FRZO', N'SN', 1.5, 0),
    (N'FISH', 11, N'COO1', N'SN', 1.5, 0),
    (N'FISH', 12, N'COO2', N'SN', 1.5, 0),
    (N'LINI', 1, N'LSNP', N'SN', 1.71, 0),
    (N'LINI', 2, N'SPNR', N'SN', 1, 0),
    (N'LINI', 3, N'SMLA', N'SN', 1.2, 0),
    (N'LINI', 4, N'BLLA', N'SN', 1.2, 0),
    (N'LINI', 5, N'BASA', N'OL', 1.2, 0),
    (N'LINI', 6, N'FRSA', N'OL', 1.2, 0),
    (N'LINI', 7, N'SIPA', N'SN', 1.2, 0),
    (N'LINI', 8, N'SIPO', N'OL', 0.75, 0),
    (N'LINI', 9, N'SISE', N'OL', 1, 0),
    (N'LINI', 10, N'LICA', N'SN', 1.2, 0),
    (N'OUSH', 1, N'FPFA', N'SN', 1.2, 0),
    (N'OUSH', 2, N'FPWF', N'SN', 1, 0),
    (N'OUSH', 3, N'FPZW', N'SN', 1.5, 0),
    (N'OUSH', 4, N'WEOU', N'SN', 2.4, 0),
    (N'OUSH', 5, N'POBA', N'SN', 1.2, 0),
    (N'OUSH', 6, N'POBO', N'OL', 0.6, 0),
    (N'OUSH', 7, N'FRPB', N'BR', 0.5, 0),
    (N'OUSH', 8, N'FRLA', N'SN', 1, 0),
    (N'OUSH', 9, N'FRSA', N'SN', 1.5, 0),
    (N'OUSH', 10, N'BASA', N'SN', 1.5, 0),
    (N'OUSH', 11, N'FBSO', N'SN', 1.33, 0),
    (N'OUSH', 12, N'SIPA', N'SN', 1.2, 0),
    (N'OUSH', 13, N'SISE', N'OL', 1, 0),
    (N'STOF', 1, N'SFRU', N'SN', 1, 0),
    (N'STOF', 2, N'STF1', N'SN', 1.2, 0),
    (N'STOF', 3, N'STF2', N'SN', 1.2, 0),
    (N'W/BA', 1, N'WPNP', N'SN', 0.75, 0),
    (N'W/BA', 2, N'WBIR', N'SN', 0.75, 0),
    (N'W/BA', 3, N'WBPA', N'SN', 1.33, 0),
    (N'W/BA', 4, N'WPNP', N'SN', 1.7, 0)
) AS v(ComponentCode, OperationSequence, OperationCode, MachineTypeCode, Sam, NumberOfMachines)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ComponentOperationTemplates t
    WHERE t.ComponentCode = v.ComponentCode AND t.OperationSequence = v.OperationSequence
);

SELECT ComponentCode, COUNT(*) AS OperationCount
FROM dbo.ComponentOperationTemplates
GROUP BY ComponentCode
ORDER BY ComponentCode;
