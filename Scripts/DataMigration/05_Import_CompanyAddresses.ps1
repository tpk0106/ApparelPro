<#
.SYNOPSIS
    Imports the 3 real legacy company address records from ie_setup.dbf into
    the modern CompanyAddresses table.

.DESCRIPTION
    Source: C:\AP\dbf tables\IE_SETUP.DBF (parsed directly - only 3 records,
    no prior CSV export existed for this file). Legacy fields ADD_NO,
    COMP_NAME, P_ADD1, P_ADD2, P_ADD3, TEL_1, FAX_1, TIN_NO, EXP_REG_NO.

    Mapping note: the legacy file has only 3 generic address lines
    (P_ADD1/2/3), no separate City/PostCode fields. Mapped as
    P_ADD1->Address1, P_ADD2->Address2, P_ADD3->Country (it consistently
    holds "SRI LANKA" text); City and PostCode are left blank rather than
    guessing a split of P_ADD2 - the legacy system never captured those
    separately, so inventing a split would misrepresent the source data.

    An existing row already has AddressNo=1 ("BROADSWORD APPAREL") that does
    not match any of these 3 legacy records - per explicit decision
    (2026-09-11), that row is kept as-is and these 3 are added alongside it,
    accepting a duplicate AddressNo=1. There is no unique constraint on
    AddressNo in the schema.

    Idempotent on (AddressNo, CompanyName): skips a row if that exact pair
    already exists, so re-running after a partial failure is safe.

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

# Hand-mapped from ie_setup.dbf (parsed directly - see script header for mapping rationale).
$records = @(
    [PSCustomObject]@{
        AddressNo = 1
        CompanyName = "FAVOURITE GARMENTS (PVT) LTD"
        Address1 = "NO.72, CHATTAM STREET,"
        Address2 = "FORT  COLOMBO 01,"
        City = ""
        PostCode = ""
        Country = "SRI LANKA ."
        TelNos = "622784, 635688"
        FaxNos = "623029"
        TinNo = ""
        ExportRegNo = "989212"
    },
    [PSCustomObject]@{
        AddressNo = 2
        CompanyName = "PTK ENTERISES LTD"
        Address1 = "822 PINNAWALA ROAD"
        Address2 = "RAMBUKKANA"
        City = ""
        PostCode = ""
        Country = ""
        TelNos = ""
        FaxNos = ""
        TinNo = ""
        ExportRegNo = ""
    },
    [PSCustomObject]@{
        AddressNo = 3
        CompanyName = "JJF"
        Address1 = "172, BANKSHALL STREET,"
        Address2 = "COLOMBO 11"
        City = ""
        PostCode = ""
        Country = "SRI LANKA."
        TelNos = ""
        FaxNos = ""
        TinNo = ""
        ExportRegNo = ""
    }
)

Write-Host "Loading existing (AddressNo, CompanyName) pairs for idempotent skip-check..."
$existingCmd = $connection.CreateCommand()
$existingCmd.CommandText = "SELECT AddressNo, CompanyName FROM CompanyAddresses"
$reader = $existingCmd.ExecuteReader()
$existingKeys = New-Object System.Collections.Generic.HashSet[string]
while ($reader.Read()) {
    [void]$existingKeys.Add("$($reader.GetInt32(0))|$($reader.GetString(1))")
}
$reader.Close()

$insertSql = @"
INSERT INTO CompanyAddresses
    (AddressNo, CompanyName, Address1, Address2, City, PostCode, Country, TelNos, FaxNos, TinNo, ExportRegNo)
VALUES
    (@AddressNo, @CompanyName, @Address1, @Address2, @City, @PostCode, @Country, @TelNos, @FaxNos, @TinNo, @ExportRegNo)
"@

$inserted = 0
$skipped = 0

$transaction = $connection.BeginTransaction()
try {
    foreach ($rec in $records) {
        $key = "$($rec.AddressNo)|$($rec.CompanyName)"
        if ($existingKeys.Contains($key)) {
            Write-Host "  Skipping AddressNo $($rec.AddressNo) ($($rec.CompanyName)) - already present."
            $skipped++
            continue
        }

        $cmd = $connection.CreateCommand()
        $cmd.Transaction = $transaction
        $cmd.CommandText = $insertSql
        [void]$cmd.Parameters.AddWithValue("@AddressNo", $rec.AddressNo)
        [void]$cmd.Parameters.AddWithValue("@CompanyName", $rec.CompanyName)
        [void]$cmd.Parameters.AddWithValue("@Address1", $rec.Address1)
        [void]$cmd.Parameters.AddWithValue("@Address2", $rec.Address2)
        [void]$cmd.Parameters.AddWithValue("@City", $rec.City)
        [void]$cmd.Parameters.AddWithValue("@PostCode", $rec.PostCode)
        [void]$cmd.Parameters.AddWithValue("@Country", $rec.Country)
        [void]$cmd.Parameters.AddWithValue("@TelNos", $rec.TelNos)
        [void]$cmd.Parameters.AddWithValue("@FaxNos", $rec.FaxNos)
        [void]$cmd.Parameters.AddWithValue("@TinNo", $rec.TinNo)
        [void]$cmd.Parameters.AddWithValue("@ExportRegNo", $rec.ExportRegNo)
        [void]$cmd.ExecuteNonQuery()

        Write-Host "  Inserted AddressNo $($rec.AddressNo): $($rec.CompanyName)"
        $inserted++
    }
    $transaction.Commit()
}
catch {
    $transaction.Rollback()
    $connection.Close()
    throw
}

$connection.Close()

Write-Host ""
Write-Host "Import complete."
Write-Host "  Inserted: $inserted"
Write-Host "  Skipped (exists): $skipped"
