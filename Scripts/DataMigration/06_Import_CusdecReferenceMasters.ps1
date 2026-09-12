<#
.SYNOPSIS
    Imports the 9 CUSDEC I/II reference-code masters from their legacy DBF
    tables into the modern SQL tables.

.DESCRIPTION
    Sources (all parsed directly - small tables, no prior CSV export):
      C:\AP\dbf tables\IE_COFF.DBF  -> ClearanceOffices   (COFF_CD, DESC)
      C:\AP\dbf tables\IE_PAY.DBF   -> PaymentTerms       (PAY_CD, DESC)
      C:\AP\dbf tables\IE_TRAN.DBF  -> TransportModes     (TRAN_CD, DESC)
      C:\AP\dbf tables\IE_DTAX.DBF  -> DutyTaxCodes       (DTAX_CD, DESC)
      C:\AP\dbf tables\IE_TBAS.DBF  -> TaxBaseCodes       (TBASE_CD, DESC)
      C:\AP\dbf tables\IE_AGRE.DBF  -> AgreementCodes     (AGRE_CD, DESC)
      C:\AP\dbf tables\IE_COMM.DBF  -> CommodityCodes     (COMM_CD, DESC)
      C:\AP\dbf tables\IE_CPRO.DBF  -> CustomsProcedureCodes (CPROC_CD, DESC)
      C:\AP\dbf tables\IE_DOCU.DBF  -> DocumentTypes      (DOC_NO, DESC, DOC_TYPE)

    IE_DOCU.DBF has duplicate (DOC_NO, DOC_TYPE) rows in the legacy data
    itself (e.g. four identical 380/WW/COMMERCIAL INVOICE rows) - the real
    document-type list is only 6 distinct (DocNo, DocTypeCode) pairs;
    deduplicated by hand below rather than importing raw duplicates that
    would violate DocumentTypes' (DocNo, DocTypeCode) unique index.

    Idempotent on the natural key of each table (Code for the 8 Code-keyed
    masters, (DocNo, DocTypeCode) for DocumentTypes) - skips a row if that
    key already exists, so re-running after a partial failure is safe.

.PARAMETER SqlServer
.PARAMETER Database
#>
param(
    [string]$SqlServer = "THUSITHPC\SQLEXPRESS",
    [string]$Database = "ApparelPro"
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName "System.Data"
$connectionString = "Server=$SqlServer;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()

function Import-CodeDescriptionTable {
    param(
        [string]$TableName,
        [array]$Records
    )
    Write-Host "--- $TableName ---"
    $existingCmd = $connection.CreateCommand()
    $existingCmd.CommandText = "SELECT Code FROM $TableName"
    $reader = $existingCmd.ExecuteReader()
    $existingKeys = New-Object System.Collections.Generic.HashSet[string]
    while ($reader.Read()) { [void]$existingKeys.Add($reader.GetString(0)) }
    $reader.Close()

    $inserted = 0; $skipped = 0
    $transaction = $connection.BeginTransaction()
    try {
        foreach ($rec in $Records) {
            if ($existingKeys.Contains($rec.Code)) {
                Write-Host "  Skipping $($rec.Code) - already present."
                $skipped++
                continue
            }
            $cmd = $connection.CreateCommand()
            $cmd.Transaction = $transaction
            $cmd.CommandText = "INSERT INTO $TableName (Code, Description) VALUES (@Code, @Description)"
            [void]$cmd.Parameters.AddWithValue("@Code", $rec.Code)
            [void]$cmd.Parameters.AddWithValue("@Description", $rec.Description)
            [void]$cmd.ExecuteNonQuery()
            Write-Host "  Inserted $($rec.Code): $($rec.Description)"
            $inserted++
        }
        $transaction.Commit()
    } catch {
        $transaction.Rollback()
        $connection.Close()
        throw
    }
    Write-Host "  Inserted: $inserted, Skipped: $skipped"
    Write-Host ""
}

Import-CodeDescriptionTable -TableName "ClearanceOffices" -Records @(
    [PSCustomObject]@{ Code = "B1Z1"; Description = "EXPORTS OFFICE [BIYAGAMA]" }
    [PSCustomObject]@{ Code = "B1Y1"; Description = "IMPORT OFFICE [BIYAGAMA]" }
    [PSCustomObject]@{ Code = "CBZ1"; Description = "EXPORTS OFFICE [COLOMBO]" }
    [PSCustomObject]@{ Code = "CBY1"; Description = "IMPORTS OFFICE [COLOMBO]" }
    [PSCustomObject]@{ Code = "CMB1"; Description = "FRONTIER OFFICE" }
    [PSCustomObject]@{ Code = "KTE1"; Description = "FRONTIER OFFICE" }
    [PSCustomObject]@{ Code = "CCTM"; Description = "COLOMBO CUSTOMS" }
)

Import-CodeDescriptionTable -TableName "PaymentTerms" -Records @(
    [PSCustomObject]@{ Code = "ADP"; Description = "ADVANCE PAYMENT" }
    [PSCustomObject]@{ Code = "CNB"; Description = "CONSIGNMENT BASIS" }
    [PSCustomObject]@{ Code = "LC"; Description = "LETTER OF CREDIT" }
    [PSCustomObject]@{ Code = "SCH"; Description = "SPOT CASH" }
)

Import-CodeDescriptionTable -TableName "TransportModes" -Records @(
    [PSCustomObject]@{ Code = "11"; Description = "SHIP" }
    [PSCustomObject]@{ Code = "12"; Description = "E-13" }
    [PSCustomObject]@{ Code = "13"; Description = "OCEAN VESSEL" }
    [PSCustomObject]@{ Code = "14"; Description = "SEA MAIL" }
    [PSCustomObject]@{ Code = "20"; Description = "RAIL" }
    [PSCustomObject]@{ Code = "15"; Description = "CRUISE SHIP" }
)

Import-CodeDescriptionTable -TableName "DutyTaxCodes" -Records @(
    [PSCustomObject]@{ Code = "CED"; Description = "CED" }
    [PSCustomObject]@{ Code = "CID"; Description = "CID" }
    [PSCustomObject]@{ Code = "CIS"; Description = "CIS" }
    [PSCustomObject]@{ Code = "EIC"; Description = "EIC" }
)

Import-CodeDescriptionTable -TableName "TaxBaseCodes" -Records @(
    [PSCustomObject]@{ Code = "BTL"; Description = "BOTTLE" }
    [PSCustomObject]@{ Code = "HPW"; Description = "HPW" }
    [PSCustomObject]@{ Code = "CRT"; Description = "CRT" }
    [PSCustomObject]@{ Code = "CMT"; Description = "CMT" }
)

Import-CodeDescriptionTable -TableName "AgreementCodes" -Records @(
    [PSCustomObject]@{ Code = "01"; Description = "BANGLADESH" }
    [PSCustomObject]@{ Code = "02"; Description = "BRAZIL" }
    [PSCustomObject]@{ Code = "03"; Description = "CUBA" }
    [PSCustomObject]@{ Code = "04"; Description = "CANADA" }
)

Import-CodeDescriptionTable -TableName "CommodityCodes" -Records @(
    [PSCustomObject]@{ Code = "10000001"; Description = "1" }
    [PSCustomObject]@{ Code = "20000002"; Description = "2" }
    [PSCustomObject]@{ Code = "30000003"; Description = "3" }
)

Import-CodeDescriptionTable -TableName "CustomsProcedureCodes" -Records @(
    [PSCustomObject]@{ Code = "E100"; Description = "E100" }
    [PSCustomObject]@{ Code = "E101"; Description = "E101" }
    [PSCustomObject]@{ Code = "E200"; Description = "E200" }
    [PSCustomObject]@{ Code = "E201"; Description = "E201" }
    [PSCustomObject]@{ Code = "E203"; Description = "E203" }
    [PSCustomObject]@{ Code = "R380"; Description = "R/380" }
)

Write-Host "--- DocumentTypes ---"
$docRecords = @(
    [PSCustomObject]@{ DocNo = "380"; DocTypeCode = "WW"; Description = "COMMERCIAL INVOICE" }
    [PSCustomObject]@{ DocNo = "380"; DocTypeCode = "EXE"; Description = "COMMERCIAL INVOICE" }
    [PSCustomObject]@{ DocNo = "380"; DocTypeCode = "YY"; Description = "COMMERCIAL INVOICE" }
    [PSCustomObject]@{ DocNo = "955"; DocTypeCode = "."; Description = "ATAC" }
    [PSCustomObject]@{ DocNo = "740"; DocTypeCode = "."; Description = "AIRWAY BILL" }
    [PSCustomObject]@{ DocNo = "810"; DocTypeCode = "."; Description = "AL" }
)
$existingCmd = $connection.CreateCommand()
$existingCmd.CommandText = "SELECT DocNo, DocTypeCode FROM DocumentTypes"
$reader = $existingCmd.ExecuteReader()
$existingDocKeys = New-Object System.Collections.Generic.HashSet[string]
while ($reader.Read()) { [void]$existingDocKeys.Add("$($reader.GetString(0))|$($reader.GetString(1))") }
$reader.Close()

$inserted = 0; $skipped = 0
$transaction = $connection.BeginTransaction()
try {
    foreach ($rec in $docRecords) {
        $key = "$($rec.DocNo)|$($rec.DocTypeCode)"
        if ($existingDocKeys.Contains($key)) {
            Write-Host "  Skipping $key - already present."
            $skipped++
            continue
        }
        $cmd = $connection.CreateCommand()
        $cmd.Transaction = $transaction
        $cmd.CommandText = "INSERT INTO DocumentTypes (DocNo, DocTypeCode, Description) VALUES (@DocNo, @DocTypeCode, @Description)"
        [void]$cmd.Parameters.AddWithValue("@DocNo", $rec.DocNo)
        [void]$cmd.Parameters.AddWithValue("@DocTypeCode", $rec.DocTypeCode)
        [void]$cmd.Parameters.AddWithValue("@Description", $rec.Description)
        [void]$cmd.ExecuteNonQuery()
        Write-Host "  Inserted $key`: $($rec.Description)"
        $inserted++
    }
    $transaction.Commit()
} catch {
    $transaction.Rollback()
    $connection.Close()
    throw
}
Write-Host "  Inserted: $inserted, Skipped: $skipped"

$connection.Close()
Write-Host ""
Write-Host "All CUSDEC reference master imports complete."
