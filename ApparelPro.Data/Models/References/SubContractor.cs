namespace ApparelPro.Data.Models.References
{
    // Replicates od_scref.dbf (Sub Contractor Reference). Built as a prerequisite for
    // Additional Issue Note (AIN) - legacy IN_AIN1.PRG validates its Sub Contractor's
    // Code field against this exact table ("seek m_subcnt" on od_scref) before allowing
    // an AIN to be saved. No entity/service/screen existed for this anywhere in the app
    // before now (previously flagged as an unbuilt gap while building the Trim Sheet
    // Report's Sub Contract costing section) - kept intentionally minimal (Code + Name),
    // matching the AdditionalCost/Basis/Unit simple reference-domain template, per
    // explicit user decision (2026-08-09) rather than building out full Sub Contract
    // costing (od_subc) at the same time.
    public class SubContractor
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
