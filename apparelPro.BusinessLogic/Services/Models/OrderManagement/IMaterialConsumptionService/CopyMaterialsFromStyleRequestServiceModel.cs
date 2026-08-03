namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService
{
    // Request to bulk-copy every StyleMaterialConsumptionLedger + StyleMaterialCostProfiles
    // line from one Buyer/Order/Type/Style (source) into another (target). Mirrors the legacy
    // OD_TPDT1.PRG "Copy data from previous Style" procedure: Quantity/Garment, Price and
    // Currency are copied as real working values (not zeroed), Color/Size are copied as-is
    // with no validation, and any item that already exists in the target is skipped.
    public class CopyMaterialsFromStyleRequestServiceModel
    {
        public int SourceBuyerCode { get; set; }
        public string SourceOrder { get; set; } = null!;
        public int SourceTypeCode { get; set; }
        public string SourceStyleCode { get; set; } = null!;

        public int TargetBuyerCode { get; set; }
        public string TargetOrder { get; set; } = null!;
        public int TargetTypeCode { get; set; }
        public string TargetStyleCode { get; set; } = null!;
    }
}
