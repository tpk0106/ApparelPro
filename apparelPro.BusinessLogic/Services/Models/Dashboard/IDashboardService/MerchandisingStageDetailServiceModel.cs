namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 0 - no stored "merchandising complete" flag exists anywhere in
    // the schema, so this is inferred: a Style row always exists (that's
    // the anchor record itself), and the two real gates are whether it has
    // any Colour/Size breakdown rows and any Material Consumption rows.
    public class MerchandisingStageDetailServiceModel
    {
        public bool StyleSaved { get; set; } = true;
        public bool BreakdownDone { get; set; }
        public bool ConsumptionDone { get; set; }
    }
}
