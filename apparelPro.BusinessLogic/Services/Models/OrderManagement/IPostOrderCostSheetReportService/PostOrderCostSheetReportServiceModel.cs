namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IPostOrderCostSheetReportService
{
    // One row per Type/Style under the Buyer/Order (od_style loop at the top of
    // OD_PCOST.PRG's report body).
    public class PostOrderCostSheetStyleServiceModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
    }

    // sect_array[] - total quantity produced per production Section, across every
    // Type/Style under this Buyer/Order. IsFinal marks the section whose quantity is
    // used as the "pieces produced" denominator for every per-piece/per-dozen cost
    // below, and as the basis for Total Value Of Sales.
    public class PostOrderCostSheetSectionQuantityServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public string SectionDescription { get; set; } = "";
        public bool IsFinal { get; set; }
        public decimal Quantity { get; set; }
    }

    // One row per raw-material/accessory/packing stock category (Stock.StockCode -
    // the first 2 characters of the item code), matching OD_PCOST.PRG's grouped
    // "@ln say m_desc" category header + subtotal loop.
    public class PostOrderCostSheetMaterialGroupServiceModel
    {
        public string StockCategoryCode { get; set; } = null!;
        public string StockCategoryDescription { get; set; } = "";
        public decimal PerPieceCost { get; set; }
        public decimal PerDozenCost { get; set; }
        public decimal TotalValue { get; set; }
    }

    // One row per Additional Cost code (od_acost), matching OD_PCOST.PRG's od_aitm
    // grouped loop. Unlike the Materials group above, PerPieceCost here is the raw
    // summed unit price (legacy prints m_price directly, not divided by section
    // quantity) - additional-cost items are already priced per garment at source.
    public class PostOrderCostSheetAdditionalCostGroupServiceModel
    {
        public string AdditionalCostCode { get; set; } = null!;
        public string AdditionalCostDescription { get; set; } = "";
        public decimal PerPieceCost { get; set; }
        public decimal PerDozenCost { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class PostOrderCostSheetReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string CurrencyCode { get; set; } = "";
        public string BasisCode { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public decimal TotalOrderQuantity { get; set; }
        public List<PostOrderCostSheetStyleServiceModel> Styles { get; set; } = new();
        // Weighted average unit price across every style row (m_totval / m_totqty).
        public decimal AverageUnitPrice { get; set; }

        // User-supplied report parameters (legacy prompts for these at print time).
        public decimal PercentOfTotalValue { get; set; } // finance-charge %
        public decimal FreightCharges { get; set; }
        public DateTime? ActualShippedDate { get; set; }

        // Delivery-on-document date (PartShipment.ShipDate for this Buyer/Order).
        public DateTime? DeliveryOnDocumentDate { get; set; }
        // Earliest DailyProductionEntry date across every section (production start).
        public DateTime? ProductionStartDate { get; set; }

        public List<PostOrderCostSheetSectionQuantityServiceModel> SectionQuantities { get; set; } = new();
        // sect_array[final] - pieces actually produced, the denominator for every
        // per-piece/per-dozen figure below.
        public decimal FinalSectionQuantity { get; set; }
        public decimal TotalValueOfSales { get; set; }

        public List<PostOrderCostSheetMaterialGroupServiceModel> MaterialGroups { get; set; } = new();
        public decimal MaterialsPerPieceCost { get; set; }
        public decimal MaterialsPerDozenCost { get; set; }
        public decimal MaterialsTotalValue { get; set; }

        public decimal ProductionCostPerPiece { get; set; } // PRODUCTION COST (In House)
        public decimal ProductionCostPerDozen { get; set; }
        public decimal ProductionCostTotalValue { get; set; }

        public List<PostOrderCostSheetAdditionalCostGroupServiceModel> AdditionalCostGroups { get; set; } = new();
        public decimal AdditionalCostPerPiece { get; set; }
        public decimal AdditionalCostPerDozen { get; set; }
        public decimal AdditionalCostTotalValue { get; set; }

        public decimal SubContractPerPiece { get; set; }
        public decimal SubContractPerDozen { get; set; }
        public decimal SubContractTotalValue { get; set; }

        // T O T A L = Production Cost + Additional Cost + Sub Contract.
        public decimal ProductionTotalPerPiece { get; set; }
        public decimal ProductionTotalPerDozen { get; set; }
        public decimal ProductionTotalValue { get; set; }

        // G R A N D  T O T A L = Production Total + Materials.
        public decimal GrandTotalPerPiece { get; set; }
        public decimal GrandTotalPerDozen { get; set; }
        public decimal GrandTotalValue { get; set; }

        public decimal GrossProfit { get; set; } // TotalValueOfSales - GrandTotalValue
        public decimal FinanceCharges { get; set; } // MaterialsTotalValue/100 * PercentOfTotalValue

        // No of days utilised for production - distinct dates with non-zero final-section
        // output. Average day production - FinalSectionQuantity / days, rounded.
        public int DaysUtilised { get; set; }
        public decimal AverageDayProduction { get; set; }

        public decimal NetProfit { get; set; } // GrossProfit - (FinanceCharges + FreightCharges)
        public decimal NetProfitOnSalesPercent { get; set; }
    }
}
