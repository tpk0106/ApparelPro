namespace apparelPro.BusinessLogic.Services.Models.Production.IEndOfProductionConfirmationService
{
    // Replicates PR_ENDPR.PRG's "END OF PRODUCTION CONFIRMATION" screen
    // (Production Control -> End of Production Confirmation). Sets
    // Style.ProductionEndDate (legacy od_style.p_end_date) once production
    // entries exist for the style.
    //
    // Legacy checked entry existence against PR_MPROD, a cached running-total
    // table kept only for Clipper's own performance - the modern equivalent
    // is checking DailyProductionEntries directly, which is functionally
    // identical and needs no separate cache.
    public class EndOfProductionStatusServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public DateOnly? CurrentProductionEndDate { get; set; }
        public bool HasProductionEntries { get; set; }
    }
}
