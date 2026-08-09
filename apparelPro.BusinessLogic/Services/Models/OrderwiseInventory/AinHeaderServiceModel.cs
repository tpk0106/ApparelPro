namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class AinHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Legacy IN_AIN1.PRG "Sub Cont." field (m_subcnt) - validated against
        // SubContractors before commit.
        public string SubContractorCode { get; set; } = null!;

        // Legacy IN_AIN1.PRG "Process" field (m_acost) - validated against AdditionalCosts,
        // then against GarmentAdditionalCosts to confirm this Buyer/Order actually has this
        // Additional Process assigned (mirrors legacy's od_aitm seek).
        public string AdditionalProcessCode { get; set; } = null!;

        // Server-assigned during commit via DocumentSequences (NoteType "AIN") - never
        // trusted from client input, same convention as every other note type.
        public string AinNumber { get; set; } = null!;
    }
}
