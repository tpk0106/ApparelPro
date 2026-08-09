namespace ApparelPro.Data.Models.OrderManagement.SubContracting
{
    // Replicates od_subc1.dbf ("Sub Contracts" - Order Management -> D. Sub Contracts in
    // legacy OD_MENU.PRG, chk_pass('od_subc1')). Assigns a quantity of a given
    // Buyer/Order/Type/Style to a Sub Contractor (see References.SubContractor) for outwork,
    // at an agreed cost per garment, tracked against how much has physically been received
    // back so far.
    //
    // NOTE: the actual od_subc1.prg source was not available in the legacy reference set
    // (only referenced by name in OD_MENU.PRG, plus the OD_SUBC1.DBF structure itself) - per
    // the Zero-Assumption Boundary Rule, the following were confirmed with the user rather
    // than assumed (2026-08-09):
    //   - ReceivedQuantity is a plain, manually-editable field on this same row - no separate
    //     receipt-transaction table/program exists anywhere in the legacy source, so none was
    //     invented here.
    //   - Total SubQuantity across all Sub Contract rows for a style is soft-warned (never
    //     blocked) against the style's ordered quantity (References.Style.Quantity) - see
    //     SubContractService.GetQuantityWarningAsync.
    public class SubContract
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string SubContractorCode { get; set; } = null!; // SUB_CODE - FK to SubContractor.Code

        public decimal SubQuantity { get; set; }      // SUB_QTY - quantity assigned to this sub-contractor
        public decimal CostPerGarment { get; set; }    // COST_P_GAR
        public string Currency { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal ReceivedQuantity { get; set; }  // RECVD_QTY - manually maintained (see class comment)
    }
}
