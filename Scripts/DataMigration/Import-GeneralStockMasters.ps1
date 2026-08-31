<#
.SYNOPSIS
    Imports legacy gi_stmst.dbf data (exported as GI_STMST.csv) into the modern
    GeneralStockMasters table.

.DESCRIPTION
    Source file is semicolon-delimited, NOT RFC4180 CSV (fields are not quote-escaped -
    a literal " character inside an item code, e.g. 1/2", is just data, not a quote).
    Columns: STORE_CD;ITEM_CD;UNIT;QTY_IN_HD;SHDW_BAL;VALUE;CURR;DAM_QTY;RO_LEVEL;RO_QTY;
    MIN_STOCK;MAX_STOCK;CPU (CPU has no equivalent column on GeneralStockMasters and is
    ignored).

    Two legacy currency codes in the source data have no modern equivalent and are
    mapped per an explicit decision (2026-08-28): US$ -> USD, SLR -> LKR (LKR is
    inserted into Currencies if missing - it's the correct ISO code for the Sri Lankan
    Rupee that SLR was standing in for; USD already exists). Two legacy unit codes (BAR,
    MTS) have no modern equivalent at all and are inserted into Units verbatim, since
    nothing else represents Bar/Meters.

    Idempotent: rows already present for a given (StoreCode, ItemCode) pair (the table's
    real unique index) are skipped, so re-running after a partial failure is safe.

.PARAMETER CsvPath
    Path to GI_STMST.csv. Defaults to the known location used for this migration.

.PARAMETER SqlServer
    SQL Server instance. Defaults to THUSITHPC\SQLEXPRESS.

.PARAMETER Database
    Database name. Defaults to ApparelPro.

.EXAMPLE
    .\Import-GeneralStockMasters.ps1
    .\Import-GeneralStockMasters.ps1 -CsvPath "C:\AppPro-Claude\gi\GI_STMST.csv"
#>
param(
    [string]$CsvPath = "C:\AppPro-Claude\gi\GI_STMST.csv",
    [string]$SqlServer = "THUSITHPC\SQLEXPRESS",
    [string]$Database = "ApparelPro"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $CsvPath)) {
    throw "CSV file not found: $CsvPath"
}

# Legacy currency code -> modern Currencies.Code. Extend this map if future imports
# surface other unmapped legacy codes.
$currencyMap = @{
    'US$' = 'USD'
    'SLR' = 'LKR'
}

# Legacy unit codes with no modern equivalent - inserted into Units verbatim.
$unitsToEnsure = @{
    'BAR' = 'BAR'
    'MTS' = 'METERS'
}

Add-Type -AssemblyName "System.Data"
$connectionString = "Server=$SqlServer;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()

function Invoke-NonQuery {
    param([string]$Sql, [hashtable]$Params = @{})
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = $Sql
    foreach ($key in $Params.Keys) {
        [void]$cmd.Parameters.AddWithValue("@$key", $Params[$key])
    }
    return $cmd.ExecuteNonQuery()
}

function Get-ScalarSet {
    param([string]$Sql)
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = $Sql
    $reader = $cmd.ExecuteReader()
    $set = New-Object System.Collections.Generic.HashSet[string]
    while ($reader.Read()) { [void]$set.Add([string]$reader.GetValue(0)) }
    $reader.Close()
    return $set
}

Write-Host "Ensuring reference data (Currencies/Units) covers the legacy codes used in the CSV..."

# --- Ensure LKR exists (USD already does) ---
$existingCurrencyCodes = Get-ScalarSet "SELECT Code FROM Currencies"
if (-not $existingCurrencyCodes.Contains('LKR')) {
    Invoke-NonQuery -Sql "INSERT INTO Currencies (Code, Name, CountryCode, Minor) VALUES (@Code, @Name, @CountryCode, @Minor)" `
        -Params @{ Code = 'LKR'; Name = 'Sri Lanka Rupee'; CountryCode = 'LKA'; Minor = '' }
    Write-Host "  Inserted Currency LKR (Sri Lanka Rupee)."
}

# --- Ensure BAR/MTS units exist ---
$existingUnitCodes = Get-ScalarSet "SELECT Code FROM Units"
foreach ($code in $unitsToEnsure.Keys) {
    if (-not $existingUnitCodes.Contains($code)) {
        Invoke-NonQuery -Sql "INSERT INTO Units (Code, Description) VALUES (@Code, @Description)" `
            -Params @{ Code = $code; Description = $unitsToEnsure[$code] }
        Write-Host "  Inserted Unit $code ($($unitsToEnsure[$code]))."
    }
}

Write-Host "Loading existing (StoreCode, ItemCode) keys for idempotent skip-check..."
$existingKeysCmd = $connection.CreateCommand()
$existingKeysCmd.CommandText = "SELECT StoreCode, ItemCode FROM GeneralStockMasters"
$existingKeysReader = $existingKeysCmd.ExecuteReader()
$existingKeys = New-Object System.Collections.Generic.HashSet[string]
while ($existingKeysReader.Read()) {
    [void]$existingKeys.Add("$($existingKeysReader.GetString(0))|$($existingKeysReader.GetString(1))")
}
$existingKeysReader.Close()

Write-Host "Parsing $CsvPath ..."
# Read raw lines rather than Import-Csv: the source has no quote-escaping at all, so a
# literal " inside a field (e.g. item code 1/2") must never be treated as a CSV quote.
$lines = Get-Content -Path $CsvPath -Encoding UTF8
if ($lines.Count -lt 2) { throw "CSV file appears to have no data rows." }

$header = $lines[0] -split ';'
$expectedHeader = @('STORE_CD','ITEM_CD','UNIT','QTY_IN_HD','SHDW_BAL','VALUE','CURR','DAM_QTY','RO_LEVEL','RO_QTY','MIN_STOCK','MAX_STOCK','CPU')
for ($i = 0; $i -lt $expectedHeader.Count; $i++) {
    if ($header[$i] -ne $expectedHeader[$i]) {
        throw "Unexpected CSV header at column $($i+1): expected '$($expectedHeader[$i])', found '$($header[$i])'. Aborting - column mapping below assumes the known layout."
    }
}

function ConvertTo-Decimal {
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return 0 }
    $parsed = 0.0
    if (-not [double]::TryParse($Value, [ref]$parsed)) { return 0 }
    return $parsed
}

$insertSql = @"
INSERT INTO GeneralStockMasters
    (StoreCode, ItemCode, Unit, QtyInHand, ShadowBalance, Value, Currency, DamagedQuantity, ReorderLevel, ReorderQuantity, MinStock, MaxStock)
VALUES
    (@StoreCode, @ItemCode, @Unit, @QtyInHand, @ShadowBalance, @Value, @Currency, @DamagedQuantity, @ReorderLevel, @ReorderQuantity, @MinStock, @MaxStock)
"@

$inserted = 0
$skippedExisting = 0
$skippedBadRow = 0
$unmappedCurrencies = New-Object System.Collections.Generic.HashSet[string]

$transaction = $connection.BeginTransaction()
try {
    for ($lineIndex = 1; $lineIndex -lt $lines.Count; $lineIndex++) {
        $line = $lines[$lineIndex]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        $fields = $line -split ';'
        if ($fields.Count -lt 12) {
            Write-Warning "Line $($lineIndex+1): expected at least 12 fields, found $($fields.Count). Skipping: $line"
            $skippedBadRow++
            continue
        }

        $storeCode = $fields[0].Trim()
        $itemCode  = $fields[1].Trim()
        $unit      = $fields[2].Trim()
        $qtyInHand = ConvertTo-Decimal $fields[3]
        $shadowBal = ConvertTo-Decimal $fields[4]
        $value     = ConvertTo-Decimal $fields[5]
        $currRaw   = $fields[6].Trim()
        $damQty    = ConvertTo-Decimal $fields[7]
        $roLevel   = ConvertTo-Decimal $fields[8]
        $roQty     = ConvertTo-Decimal $fields[9]
        $minStock  = ConvertTo-Decimal $fields[10]
        $maxStock  = ConvertTo-Decimal $fields[11]
        # $fields[12] (CPU) intentionally ignored - no equivalent column.

        if ([string]::IsNullOrWhiteSpace($storeCode) -or [string]::IsNullOrWhiteSpace($itemCode)) {
            Write-Warning "Line $($lineIndex+1): missing StoreCode or ItemCode. Skipping: $line"
            $skippedBadRow++
            continue
        }

        $key = "$storeCode|$itemCode"
        if ($existingKeys.Contains($key)) {
            $skippedExisting++
            continue
        }

        $currency = if ($currencyMap.ContainsKey($currRaw)) { $currencyMap[$currRaw] } else { $currRaw }
        if ([string]::IsNullOrWhiteSpace($currency)) {
            Write-Warning "Line $($lineIndex+1): blank Currency for Item '$itemCode' at Store '$storeCode'. Skipping row - Currency is NOT NULL."
            $skippedBadRow++
            continue
        }
        if (-not $existingCurrencyCodes.Contains($currency) -and -not $currencyMap.ContainsValue($currency)) {
            [void]$unmappedCurrencies.Add($currency)
        }

        $cmd = $connection.CreateCommand()
        $cmd.Transaction = $transaction
        $cmd.CommandText = $insertSql
        [void]$cmd.Parameters.AddWithValue("@StoreCode", $storeCode)
        [void]$cmd.Parameters.AddWithValue("@ItemCode", $itemCode)
        [void]$cmd.Parameters.AddWithValue("@Unit", $unit)
        [void]$cmd.Parameters.AddWithValue("@QtyInHand", $qtyInHand)
        [void]$cmd.Parameters.AddWithValue("@ShadowBalance", $shadowBal)
        [void]$cmd.Parameters.AddWithValue("@Value", $value)
        [void]$cmd.Parameters.AddWithValue("@Currency", $currency)
        [void]$cmd.Parameters.AddWithValue("@DamagedQuantity", $damQty)
        [void]$cmd.Parameters.AddWithValue("@ReorderLevel", $roLevel)
        [void]$cmd.Parameters.AddWithValue("@ReorderQuantity", $roQty)
        [void]$cmd.Parameters.AddWithValue("@MinStock", $minStock)
        [void]$cmd.Parameters.AddWithValue("@MaxStock", $maxStock)
        [void]$cmd.ExecuteNonQuery()

        [void]$existingKeys.Add($key)
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
Write-Host "  Inserted        : $inserted"
Write-Host "  Skipped (exists): $skippedExisting"
Write-Host "  Skipped (bad row): $skippedBadRow"
if ($unmappedCurrencies.Count -gt 0) {
    Write-Warning "Currency codes written verbatim with no known mapping (Currencies FK would have rejected these if they didn't already exist - verify): $($unmappedCurrencies -join ', ')"
}
