namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService
{
    // Everything the printed Packing List needs - the invoice-level header
    // block (buyer/consignee/notify/bank/ports/etc., resolved the same way
    // as the Commercial Invoice print, reused rather than re-derived), the
    // line's own free-text Detail memo, and whichever one of the Carton/
    // String breakdown grids applies, grouped and ordered by size ready to
    // print as a table.
    public class PackingListPrintDetailsServiceModel
    {
        public string InvoiceNumber { get; set; } = "";
        public string? InvoiceDate { get; set; }
        public string StyleCode { get; set; } = "";
        public string NewOrder { get; set; } = "";
        public string? CarrierCode { get; set; }
        public string? ShipDate { get; set; }
        public string? LcNumber { get; set; }
        public string? LcDate { get; set; }

        public string ConsigneeName { get; set; } = "";
        public List<string> ConsigneeAddressLines { get; set; } = new();
        public string NotifyPartyName { get; set; } = "";
        public List<string> NotifyPartyAddressLines { get; set; } = new();
        public string IssuingBankName { get; set; } = "";
        public string LoadPortDescription { get; set; } = "";
        public string DestinationDescription { get; set; } = "";
        public string? Remark1 { get; set; }
        public string? Remark2 { get; set; }
        public string? Remark3 { get; set; }

        public string? Detail { get; set; }

        public List<string> Sizes { get; set; } = new();
        public List<PackingListCartonGroupServiceModel> CartonGroups { get; set; } = new();
        public List<PackingListStringGroupServiceModel> StringGroups { get; set; } = new();
    }

    public class PackingListCartonGroupServiceModel
    {
        public int FromCartonNo { get; set; }
        public int ToCartonNo { get; set; }
        public string Color { get; set; } = "";
        public int NoOfCartons { get; set; }
        public Dictionary<string, decimal> QtyBySize { get; set; } = new();
    }

    public class PackingListStringGroupServiceModel
    {
        public int BarNo { get; set; }
        public int FromStringNo { get; set; }
        public int ToStringNo { get; set; }
        public string Color { get; set; } = "";
        public Dictionary<string, decimal> QtyBySize { get; set; } = new();
    }
}
