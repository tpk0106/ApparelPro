namespace ApparelPro.WebApi.Reports.Models
{
    public class PostOrderCostSheetStyleAPIModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
    }

    public class PostOrderCostSheetSectionQuantityAPIModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = "";
        public bool IsFinal { get; set; }
        public decimal Quantity { get; set; }
    }

    public class PostOrderCostSheetMaterialGroupAPIModel
    {
        public string StockCategoryCode { get; set; } = null!;
        public string StockCategoryDescription { get; set; } = "";
        public decimal PerPieceCost { get; set; }
        public decimal PerDozenCost { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class PostOrderCostSheetAdditionalCostGroupAPIModel
    {
        public string AdditionalCostCode { get; set; } = null!;
        public string AdditionalCostDescription { get; set; } = "";
        public decimal PerPieceCost { get; set; }
        public decimal PerDozenCost { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class PostOrderCostSheetReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string CurrencyCode { get; set; } = "";
        public string BasisCode { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public decimal TotalOrderQuantity { get; set; }
        public List<PostOrderCostSheetStyleAPIModel> Styles { get; set; } = new();
        public decimal AverageUnitPrice { get; set; }

        public decimal PercentOfTotalValue { get; set; }
        public decimal FreightCharges { get; set; }
        public DateTime? ActualShippedDate { get; set; }

        public DateTime? DeliveryOnDocumentDate { get; set; }
        public DateTime? ProductionStartDate { get; set; }

        public List<PostOrderCostSheetSectionQuantityAPIModel> SectionQuantities { get; set; } = new();
        public decimal FinalSectionQuantity { get; set; }
        public decimal TotalValueOfSales { get; set; }

        public List<PostOrderCostSheetMaterialGroupAPIModel> MaterialGroups { get; set; } = new();
        public decimal MaterialsPerPieceCost { get; set; }
        public decimal MaterialsPerDozenCost { get; set; }
        public decimal MaterialsTotalValue { get; set; }

        public decimal ProductionCostPerPiece { get; set; }
        public decimal ProductionCostPerDozen { get; set; }
        public decimal ProductionCostTotalValue { get; set; }

        public List<PostOrderCostSheetAdditionalCostGroupAPIModel> AdditionalCostGroups { get; set; } = new();
        public decimal AdditionalCostPerPiece { get; set; }
        public decimal AdditionalCostPerDozen { get; set; }
        public decimal AdditionalCostTotalValue { get; set; }

        public decimal SubContractPerPiece { get; set; }
        public decimal SubContractPerDozen { get; set; }
        public decimal SubContractTotalValue { get; set; }

        public decimal ProductionTotalPerPiece { get; set; }
        public decimal ProductionTotalPerDozen { get; set; }
        public decimal ProductionTotalValue { get; set; }

        public decimal GrandTotalPerPiece { get; set; }
        public decimal GrandTotalPerDozen { get; set; }
        public decimal GrandTotalValue { get; set; }

        public decimal GrossProfit { get; set; }
        public decimal FinanceCharges { get; set; }

        public int DaysUtilised { get; set; }
        public decimal AverageDayProduction { get; set; }

        public decimal NetProfit { get; set; }
        public decimal NetProfitOnSalesPercent { get; set; }
    }
}
