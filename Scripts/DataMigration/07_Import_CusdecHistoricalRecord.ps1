<#
.SYNOPSIS
    Imports the single real historical CUSDEC I/II declaration (CusNo "8")
    from ie_cusd1/ie_cusd2/ie_cusd4.dbf into the modern CUSDEC tables.

.DESCRIPTION
    Source: C:\AP\dbf tables\IE_CUSD1.DBF (1 header record), IE_CUSD2.DBF
    (1 item line), IE_CUSD4.DBF (2 attached-document records). IE_CUSD3.DBF
    (line taxes) has 0 records - nothing to import there.

    This is the only genuinely real record found across the IE
    Documentation legacy tables during the 2026-09-13 data migration pass -
    a real 1995 export declaration (vessel "NEPTUNE RUBY", real Sri Lanka
    port/bank codes, real declarant name). Commercial Invoice (83+92
    records) and Packing List (34/226/1732 records) are real but blocked on
    the current Styles table only holding 12 unrelated test rows: their
    line items FK to (Buyer, Order, Type, Style), which this historical
    data can't satisfy. L/C Form's 4 records were found to be developer
    test data (keyboard-mashing values like "E32EWTRYRYURT6UTUYKIUKIUKUIKIUKUIKIUKULU"),
    not real history, and were deliberately excluded.

    Neither ie_cusd1.dbf nor ie_cusd2.dbf has a companion .DBT/.FPT memo
    file alongside it, so the DETAIL memo field on the line item can't be
    recovered - left blank rather than guessed.

    ie_cusd2.dbf's CPC field holds "R 380" (5 characters incl. an internal
    space) but CustomsProcedureCode is varchar(4) - the space is dropped
    ("R380") to fit; nothing in the source suggests it carries separate
    meaning.

    No FK constraints exist on CustomsDeclarationHeaders/Lines/
    AttachedDocuments, so the numeric-looking legacy codes (ExporterCode
    "00000000001" etc - internal legacy buyer-reference IDs, not the
    modern reference tables' actual codes) are carried over as-is; they
    were never resolvable against today's masters and this record predates
    the reference-code masters this project seeded anyway.

    Idempotent on CusNo for the header; skips entirely if CusNo "8"
    already exists (re-running after a partial failure is safe).

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

$existingCmd = $connection.CreateCommand()
$existingCmd.CommandText = "SELECT COUNT(*) FROM CustomsDeclarationHeaders WHERE CusNo = '8'"
$exists = [int]$existingCmd.ExecuteScalar()
if ($exists -gt 0) {
    Write-Host "CusNo '8' already exists in CustomsDeclarationHeaders - nothing to do."
    $connection.Close()
    return
}

# Hand-mapped from ie_cusd1.dbf's single real record (see script header).
$header = [PSCustomObject]@{
    CusNo = "8"
    ExporterCode = "00000000001"
    BoiRegNo = "419/25/8/92"
    ConsigneeCode = "00000000002"
    NotifyPartyCode = "00000000001"
    DeclarantCode = "00000000001"
    ClearanceOfficeCode = "CBY1"
    FrontierOfficeCode = "KTE1"
    CountryOfConsignmentCode = "SR"
    LocationOfGoods = ""
    CountryOfOriginCode = "SR"
    CountryOfDestinationCode = "EN"
    WarehouseNo = ""
    WarehousePeriod = ""
    PrecedingDocNo = ""
    VoyageNo = ""
    VoyageDate = [DateTime]::new(1995, 3, 18) # VOY_DT "950318"
    BlAwbNo = ""
    PaymentTermCode = "DAP"
    DeliveryTermCode = "FOB"
    Vessel = "NEPTUNE RUBY"
    PortOfLoadingCode = "CO"
    TransportModeCode = "13"
    PrepaymentAccountName = "FCBU011"
    PrepaymentAccountNo = "0031134-030"
    PortOfDischargeCode = "LO"
    PlaceOfDeliveryCode = "LO"
    BankCode = "7056-003"
    ReferenceNo = "D-139073"
    Remark1 = "USD 20,410.00"
    Remark2 = "CBM 12"
    Remark3 = "GALNEWA, BULNEWA"
    Remark4 = ""
    DeclarantName = "P M RANASINGHE"
    SubmittedByName = "P M RANASINGHE"
}

# Hand-mapped from ie_cusd2.dbf's single real line (matches header's Remark1
# FOB value). DETAIL memo can't be recovered - no companion memo file exists.
$line = [PSCustomObject]@{
    CusNo = "8"
    Item = "1/1"
    # ie_cusd2.dbf's CPC field holds "R 380" (5 chars incl. an internal
    # space), but CustomsProcedureCode is varchar(4) - the space is
    # dropped to fit; nothing else in the source suggests it carries
    # separate meaning.
    CustomsProcedureCode = "R380"
    CommodityCode = "6204.53"
    NetWeight = 1250.00
    GrossWeight = 1500.00
    SupplementaryUnitCode = "DOZ"
    SupplementaryQty = 0.00
    CurrencyCode = "US$"
    Fob = 20410.00
    Freight = 0.00
    Insurance = 0.00
    Other = 0.00
    ExchangeRate = 50.1700
    CountryCode = "SR"
    LicenceNo = ""
    AgreementCode = ""
    QtyDeducted = 0.00
    Value = "2,600 PCS"
    AnyOther = ""
    Detail = $null
}

# Hand-mapped from ie_cusd4.dbf's 2 records (both identical DocNo/DocType -
# carried over as-is rather than silently de-duplicated, since the source
# genuinely has 2 non-deleted rows).
$attachedDocs = @(
    [PSCustomObject]@{ CusNo = "8"; DocNo = "380"; DocTypeCode = "EXP/MAR/95/05" },
    [PSCustomObject]@{ CusNo = "8"; DocNo = "380"; DocTypeCode = "EXP/MAR/95/05" }
)

$transaction = $connection.BeginTransaction()
try {
    $headerCmd = $connection.CreateCommand()
    $headerCmd.Transaction = $transaction
    $headerCmd.CommandText = @"
INSERT INTO CustomsDeclarationHeaders
    (CusNo, ExporterCode, BoiRegNo, ConsigneeCode, NotifyPartyCode, DeclarantCode,
     ClearanceOfficeCode, FrontierOfficeCode, CountryOfConsignmentCode, LocationOfGoods,
     CountryOfOriginCode, CountryOfDestinationCode, WarehouseNo, WarehousePeriod,
     PrecedingDocNo, VoyageNo, VoyageDate, BlAwbNo, PaymentTermCode, DeliveryTermCode,
     Vessel, PortOfLoadingCode, TransportModeCode, PrepaymentAccountName, PrepaymentAccountNo,
     PortOfDischargeCode, PlaceOfDeliveryCode, BankCode, ReferenceNo,
     Remark1, Remark2, Remark3, Remark4, DeclarantName, SubmittedByName)
VALUES
    (@CusNo, @ExporterCode, @BoiRegNo, @ConsigneeCode, @NotifyPartyCode, @DeclarantCode,
     @ClearanceOfficeCode, @FrontierOfficeCode, @CountryOfConsignmentCode, @LocationOfGoods,
     @CountryOfOriginCode, @CountryOfDestinationCode, @WarehouseNo, @WarehousePeriod,
     @PrecedingDocNo, @VoyageNo, @VoyageDate, @BlAwbNo, @PaymentTermCode, @DeliveryTermCode,
     @Vessel, @PortOfLoadingCode, @TransportModeCode, @PrepaymentAccountName, @PrepaymentAccountNo,
     @PortOfDischargeCode, @PlaceOfDeliveryCode, @BankCode, @ReferenceNo,
     @Remark1, @Remark2, @Remark3, @Remark4, @DeclarantName, @SubmittedByName)
"@
    foreach ($prop in $header.PSObject.Properties) {
        [void]$headerCmd.Parameters.AddWithValue("@$($prop.Name)", $(if ($null -eq $prop.Value) { [DBNull]::Value } else { $prop.Value }))
    }
    [void]$headerCmd.ExecuteNonQuery()
    Write-Host "Inserted CustomsDeclarationHeaders.CusNo = 8"

    $lineCmd = $connection.CreateCommand()
    $lineCmd.Transaction = $transaction
    $lineCmd.CommandText = @"
INSERT INTO CustomsDeclarationLines
    (CusNo, Item, CustomsProcedureCode, CommodityCode, NetWeight, GrossWeight,
     SupplementaryUnitCode, SupplementaryQty, CurrencyCode, Fob, Freight, Insurance, Other,
     ExchangeRate, CountryCode, LicenceNo, AgreementCode, QtyDeducted, Value, AnyOther, Detail)
VALUES
    (@CusNo, @Item, @CustomsProcedureCode, @CommodityCode, @NetWeight, @GrossWeight,
     @SupplementaryUnitCode, @SupplementaryQty, @CurrencyCode, @Fob, @Freight, @Insurance, @Other,
     @ExchangeRate, @CountryCode, @LicenceNo, @AgreementCode, @QtyDeducted, @Value, @AnyOther, @Detail)
"@
    foreach ($prop in $line.PSObject.Properties) {
        [void]$lineCmd.Parameters.AddWithValue("@$($prop.Name)", $(if ($null -eq $prop.Value) { [DBNull]::Value } else { $prop.Value }))
    }
    [void]$lineCmd.ExecuteNonQuery()
    Write-Host "Inserted CustomsDeclarationLines (CusNo=8, Item=1/1)"

    foreach ($doc in $attachedDocs) {
        $docCmd = $connection.CreateCommand()
        $docCmd.Transaction = $transaction
        $docCmd.CommandText = "INSERT INTO CustomsDeclarationAttachedDocuments (CusNo, DocNo, DocTypeCode) VALUES (@CusNo, @DocNo, @DocTypeCode)"
        [void]$docCmd.Parameters.AddWithValue("@CusNo", $doc.CusNo)
        [void]$docCmd.Parameters.AddWithValue("@DocNo", $doc.DocNo)
        [void]$docCmd.Parameters.AddWithValue("@DocTypeCode", $doc.DocTypeCode)
        [void]$docCmd.ExecuteNonQuery()
    }
    Write-Host "Inserted 2 CustomsDeclarationAttachedDocuments rows"

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
