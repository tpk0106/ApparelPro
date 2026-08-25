namespace ApparelPro.Data.Models.GeneralInventory
{
    // Replicates od_stref.dbf - the list of physical stores General Inventory can move
    // stock between/from (e.g. "M-S" -> "MAIN STORES"). Distinct from References.Basis.
    public class GeneralStore
    {
        public string Code { get; set; } = null!;
        public string Description { get; set; } = "";
    }
}
