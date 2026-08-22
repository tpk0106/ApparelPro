namespace apparelPro.BusinessLogic.SystemConfiguration
{
    // Single source of truth for SystemParameters.ParameterKey string literals
    // and their fallback defaults, so a key/default pair only ever needs to
    // change in one place (previously duplicated between DailyProductionEntryService
    // and now the Dashboard aggregation service).
    public static class SystemParameterKeys
    {
        public const string ProductionContractSectionCode = "ProductionContractSectionCode";
        public const string ProductionContractSectionCodeDefault = "001";

        public const string DashboardPinnedBuyerCode = "DashboardPinnedBuyerCode";
        public const string DashboardPinnedOrder = "DashboardPinnedOrder";
        public const string DashboardPinnedTypeCode = "DashboardPinnedTypeCode";
        public const string DashboardPinnedStyleCode = "DashboardPinnedStyleCode";
    }
}
