namespace ApparelPro.WebApi.Reports.Models
{
    public class CostOfProductionMaterialLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StockCategoryCode { get; set; } = "";
        public string StockCategoryDescription { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Value { get; set; }
    }

    public class CostOfProductionAdditionalCostLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public decimal QuantityPerGarment { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal PricePerUnit { get; set; }
        public decimal Cost { get; set; }
    }

    public class CostOfProductionAdditionalCostGroupAPIModel
    {
        public string AdditionalCostCode { get; set; } = null!;
        public string AdditionalCostDescription { get; set; } = "";
        public List<CostOfProductionAdditionalCostLineAPIModel> Lines { get; set; } = new();
        public decimal TotalCost { get; set; }
    }

    public class CostOfProductionSubContractLineAPIModel
    {
        public string SubContractorCode { get; set; } = null!;
        public string SubContractorName { get; set; } = "";
        public decimal CostPerGarment { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal Cost { get; set; }
    }

    public class CostOfProductionStyleRevenueAPIModel
    {
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = "";
        public decimal EstimatedQuantity { get; set; }
        public decimal EstimatedValue { get; set; }
        public decimal ActualProducedQuantity { get; set; }
        public decimal ActualProducedValue { get; set; }
        public decimal SubContractReceivedQuantity { get; set; }
        public decimal SubContractReceivedValue { get; set; }
    }

    public class CostOfProductionLineCostAPIModel
    {
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string LineCode { get; set; } = null!;
        public string LineDescription { get; set; } = "";
        public decimal CostPerDay { get; set; }
        public decimal EstimatedDays { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ActualDays { get; set; }
        public decimal ActualCost { get; set; }
    }

    public class CostOfProductionReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string CurrencyCode { get; set; } = "";
        public decimal TotalOrderQuantity { get; set; }
        public string Unit { get; set; } = "";

        public List<CostOfProductionMaterialLineAPIModel> Materials { get; set; } = new();
        public decimal TotalMaterialsValue { get; set; }
        public decimal EstimatedMaterialsValue { get; set; }

        public List<CostOfProductionAdditionalCostGroupAPIModel> AdditionalCostGroups { get; set; } = new();
        public decimal TotalAdditionalCostValue { get; set; }
        public decimal EstimatedAdditionalCostValue { get; set; }

        public List<CostOfProductionSubContractLineAPIModel> SubContracts { get; set; } = new();
        public decimal TotalSubContractValue { get; set; }

        public List<CostOfProductionStyleRevenueAPIModel> StyleRevenues { get; set; } = new();
        public List<CostOfProductionLineCostAPIModel> LineCosts { get; set; } = new();

        public decimal EstimatedProfitMargin { get; set; }
        public decimal ActualProfitMargin { get; set; }
    }
}
