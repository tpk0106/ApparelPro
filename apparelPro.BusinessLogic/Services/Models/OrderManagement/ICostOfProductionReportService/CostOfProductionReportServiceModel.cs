namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.ICostOfProductionReportService
{
    // Replicates OD_FCOST.PRG's "COST OF PRODUCTION" report - an Estimated vs Actual
    // profitability analysis for a Buyer+Order across five cost centers: materials
    // received, additional costs, sub-contracts, style revenue, and production line
    // costing. See CostOfProductionReportService for the specific legacy quirks/defects
    // deliberately not replicated (documented at each point).

    public class CostOfProductionMaterialLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StockCategoryCode { get; set; } = ""; // first 2 chars of ItemCode - legacy's print grouping key
        public string StockCategoryDescription { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; } // to_dt_rec - supplier return
        public decimal Price { get; set; } // currency-converted to order currency
        public decimal Value { get; set; }
    }

    public class CostOfProductionAdditionalCostLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public decimal QuantityPerGarment { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal PricePerUnit { get; set; } // currency-converted
        public decimal Cost { get; set; } // price * (receivedQty / qtyPerGarment)
    }

    public class CostOfProductionAdditionalCostGroupServiceModel
    {
        public string AdditionalCostCode { get; set; } = null!;
        public string AdditionalCostDescription { get; set; } = "";
        public List<CostOfProductionAdditionalCostLineServiceModel> Lines { get; set; } = new();
        public decimal TotalCost { get; set; }
    }

    public class CostOfProductionSubContractLineServiceModel
    {
        public string SubContractorCode { get; set; } = null!;
        public string SubContractorName { get; set; } = "";
        public decimal CostPerGarment { get; set; } // currency-converted
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal Cost { get; set; }
    }

    public class CostOfProductionStyleRevenueServiceModel
    {
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = "";
        public decimal EstimatedQuantity { get; set; } // Style.Quantity
        public decimal EstimatedValue { get; set; }
        // Actual production quantity at the final production Section, aggregated live
        // from DailyProductionEntry (this codebase's established "no stored running
        // totals" convention - see DailyProductionEntry.cs).
        public decimal ActualProducedQuantity { get; set; }
        public decimal ActualProducedValue { get; set; }
        public decimal SubContractReceivedQuantity { get; set; }
        public decimal SubContractReceivedValue { get; set; }
    }

    public class CostOfProductionLineCostServiceModel
    {
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string LineCode { get; set; } = null!;
        public string LineDescription { get; set; } = "";
        public decimal CostPerDay { get; set; } // currency-converted
        public decimal EstimatedDays { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ActualDays { get; set; }
        public decimal ActualCost { get; set; }
    }

    public class CostOfProductionReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string CurrencyCode { get; set; } = "";
        public decimal TotalOrderQuantity { get; set; }
        public string Unit { get; set; } = "";

        public List<CostOfProductionMaterialLineServiceModel> Materials { get; set; } = new();
        public decimal TotalMaterialsValue { get; set; } // m_totval (actual)
        public decimal EstimatedMaterialsValue { get; set; } // m_estval

        public List<CostOfProductionAdditionalCostGroupServiceModel> AdditionalCostGroups { get; set; } = new();
        public decimal TotalAdditionalCostValue { get; set; } // m_acos_tot (actual)
        public decimal EstimatedAdditionalCostValue { get; set; } // x_add_cost

        public List<CostOfProductionSubContractLineServiceModel> SubContracts { get; set; } = new();
        public decimal TotalSubContractValue { get; set; } // m_sub_tot - legacy uses this one figure for both estimated and actual

        public List<CostOfProductionStyleRevenueServiceModel> StyleRevenues { get; set; } = new();
        public List<CostOfProductionLineCostServiceModel> LineCosts { get; set; } = new();

        public decimal EstimatedProfitMargin { get; set; }
        public decimal ActualProfitMargin { get; set; }
    }
}
