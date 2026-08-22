namespace ApparelPro.Data.Models.Production
{
    // PR_MOP2 - reusable Component -> Operation library. Seeds a new style's
    // operation breakdown the first time a component is opened with no rows
    // yet (RF_MENU.PRG > D > "Component/Op. Breakdown").
    public class ComponentOperationTemplate
    {
        public string ComponentCode { get; set; } = null!;
        public int OperationSequence { get; set; }
        public string OperationCode { get; set; } = null!;
        public string MachineTypeCode { get; set; } = null!;
        public decimal Sam { get; set; }
        public decimal NumberOfMachines { get; set; }
    }
}
