using apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
// ICommercialInvoiceService lives in apparelPro.BusinessLogic.Services -
// not implicitly visible from this Implementation.ImportExport namespace.
using apparelPro.BusinessLogic.Services;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the Packing List detail attached to one Commercial Invoice
    // line (legacy ie_pack1/ie_pack2/ie_pack3, keyed by the same composite
    // business key as CommercialInvoiceLine) - a free-text Detail memo plus
    // whichever one of the Carton/String breakdown grids applies, decided
    // by that line's own PackingMedia ("1" Carton / "2" Container), same
    // replace-the-child-rows-on-save shape as the other IE forms.
    public class PackingListService : IPackingListService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICommercialInvoiceService _commercialInvoiceService;

        public PackingListService(IMapper mapper, ApparelProDbContext apparelProDbContext, ICommercialInvoiceService commercialInvoiceService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _commercialInvoiceService = commercialInvoiceService;
        }

        public async Task<PackingListDetailServiceModel> GetByLineKeyAsync(
            string invoiceNumber, int buyerCode, string order, int typeCode, string styleCode, string newOrder)
        {
            var line = await _apparelProDbContext.PackingListLines.AsNoTracking().FirstOrDefaultAsync(l =>
                l.InvoiceNumber == invoiceNumber && l.BuyerCode == buyerCode && l.Order == order &&
                l.TypeCode == typeCode && l.StyleCode == styleCode && l.NewOrder == newOrder);

            var cartonRows = await _apparelProDbContext.PackingListCartonDetails.AsNoTracking().Where(c =>
                c.InvoiceNumber == invoiceNumber && c.BuyerCode == buyerCode && c.Order == order &&
                c.TypeCode == typeCode && c.StyleCode == styleCode && c.NewOrder == newOrder).ToListAsync();

            var stringRows = await _apparelProDbContext.PackingListStringDetails.AsNoTracking().Where(s =>
                s.InvoiceNumber == invoiceNumber && s.BuyerCode == buyerCode && s.Order == order &&
                s.TypeCode == typeCode && s.StyleCode == styleCode && s.NewOrder == newOrder).ToListAsync();

            return new PackingListDetailServiceModel
            {
                Detail = line?.Detail,
                CartonRows = _mapper.Map<List<PackingListCartonDetailServiceModel>>(cartonRows),
                StringRows = _mapper.Map<List<PackingListStringDetailServiceModel>>(stringRows),
            };
        }

        public async Task<PackingListDetailServiceModel> SaveAsync(SavePackingListServiceModel serviceModel)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var line = await _apparelProDbContext.PackingListLines.FirstOrDefaultAsync(l =>
                l.InvoiceNumber == serviceModel.InvoiceNumber && l.BuyerCode == serviceModel.BuyerCode &&
                l.Order == serviceModel.Order && l.TypeCode == serviceModel.TypeCode &&
                l.StyleCode == serviceModel.StyleCode && l.NewOrder == serviceModel.NewOrder);
            if (line == null)
            {
                line = new PackingListLine
                {
                    InvoiceNumber = serviceModel.InvoiceNumber,
                    BuyerCode = serviceModel.BuyerCode,
                    Order = serviceModel.Order,
                    TypeCode = serviceModel.TypeCode,
                    StyleCode = serviceModel.StyleCode,
                    NewOrder = serviceModel.NewOrder,
                    Detail = serviceModel.Detail,
                };
                _apparelProDbContext.PackingListLines.Add(line);
            }
            else
            {
                line.Detail = serviceModel.Detail;
            }

            var existingCartonRows = await _apparelProDbContext.PackingListCartonDetails.Where(c =>
                c.InvoiceNumber == serviceModel.InvoiceNumber && c.BuyerCode == serviceModel.BuyerCode &&
                c.Order == serviceModel.Order && c.TypeCode == serviceModel.TypeCode &&
                c.StyleCode == serviceModel.StyleCode && c.NewOrder == serviceModel.NewOrder).ToListAsync();
            _apparelProDbContext.PackingListCartonDetails.RemoveRange(existingCartonRows);

            var existingStringRows = await _apparelProDbContext.PackingListStringDetails.Where(s =>
                s.InvoiceNumber == serviceModel.InvoiceNumber && s.BuyerCode == serviceModel.BuyerCode &&
                s.Order == serviceModel.Order && s.TypeCode == serviceModel.TypeCode &&
                s.StyleCode == serviceModel.StyleCode && s.NewOrder == serviceModel.NewOrder).ToListAsync();
            _apparelProDbContext.PackingListStringDetails.RemoveRange(existingStringRows);

            await _apparelProDbContext.SaveChangesAsync();

            var newCartonRows = serviceModel.CartonRows.Select(r =>
            {
                var row = _mapper.Map<PackingListCartonDetail>(r);
                row.Id = 0;
                row.InvoiceNumber = serviceModel.InvoiceNumber;
                row.BuyerCode = serviceModel.BuyerCode;
                row.Order = serviceModel.Order;
                row.TypeCode = serviceModel.TypeCode;
                row.StyleCode = serviceModel.StyleCode;
                row.NewOrder = serviceModel.NewOrder;
                return row;
            }).ToList();
            await _apparelProDbContext.PackingListCartonDetails.AddRangeAsync(newCartonRows);

            var newStringRows = serviceModel.StringRows.Select(r =>
            {
                var row = _mapper.Map<PackingListStringDetail>(r);
                row.Id = 0;
                row.InvoiceNumber = serviceModel.InvoiceNumber;
                row.BuyerCode = serviceModel.BuyerCode;
                row.Order = serviceModel.Order;
                row.TypeCode = serviceModel.TypeCode;
                row.StyleCode = serviceModel.StyleCode;
                row.NewOrder = serviceModel.NewOrder;
                return row;
            }).ToList();
            await _apparelProDbContext.PackingListStringDetails.AddRangeAsync(newStringRows);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByLineKeyAsync(serviceModel.InvoiceNumber, serviceModel.BuyerCode, serviceModel.Order,
                serviceModel.TypeCode, serviceModel.StyleCode, serviceModel.NewOrder);
        }

        public async Task<PackingListPrintDetailsServiceModel?> GetPrintDetailsAsync(
            string invoiceNumber, int buyerCode, string order, int typeCode, string styleCode, string newOrder)
        {
            var invoicePrintDetails = await _commercialInvoiceService.GetPrintDetailsAsync(invoiceNumber);
            if (invoicePrintDetails == null) return null;
            var header = invoicePrintDetails.Header;

            var line = await _apparelProDbContext.PackingListLines.AsNoTracking().FirstOrDefaultAsync(l =>
                l.InvoiceNumber == invoiceNumber && l.BuyerCode == buyerCode && l.Order == order &&
                l.TypeCode == typeCode && l.StyleCode == styleCode && l.NewOrder == newOrder);

            var cartonRows = await _apparelProDbContext.PackingListCartonDetails.AsNoTracking().Where(c =>
                c.InvoiceNumber == invoiceNumber && c.BuyerCode == buyerCode && c.Order == order &&
                c.TypeCode == typeCode && c.StyleCode == styleCode && c.NewOrder == newOrder).ToListAsync();

            var stringRows = await _apparelProDbContext.PackingListStringDetails.AsNoTracking().Where(s =>
                s.InvoiceNumber == invoiceNumber && s.BuyerCode == buyerCode && s.Order == order &&
                s.TypeCode == typeCode && s.StyleCode == styleCode && s.NewOrder == newOrder).ToListAsync();

            var sizes = await _apparelProDbContext.ColorSizeDetails.AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode)
                .Select(d => d.Size).Distinct().ToListAsync();

            var cartonGroups = cartonRows
                .GroupBy(r => new { r.FromCartonNo, r.ToCartonNo, r.Color, r.NoOfCartons })
                .Select(g => new PackingListCartonGroupServiceModel
                {
                    FromCartonNo = g.Key.FromCartonNo,
                    ToCartonNo = g.Key.ToCartonNo,
                    Color = g.Key.Color,
                    NoOfCartons = g.Key.NoOfCartons,
                    QtyBySize = g.ToDictionary(r => r.Size, r => r.Qty),
                }).ToList();

            var stringGroups = stringRows
                .GroupBy(r => new { r.BarNo, r.FromStringNo, r.ToStringNo, r.Color })
                .Select(g => new PackingListStringGroupServiceModel
                {
                    BarNo = g.Key.BarNo,
                    FromStringNo = g.Key.FromStringNo,
                    ToStringNo = g.Key.ToStringNo,
                    Color = g.Key.Color,
                    QtyBySize = g.ToDictionary(r => r.Size, r => r.Qty),
                }).ToList();

            return new PackingListPrintDetailsServiceModel
            {
                InvoiceNumber = invoiceNumber,
                InvoiceDate = header.InvoiceDate.ToString("dd/MM/yyyy"),
                StyleCode = styleCode,
                NewOrder = newOrder,
                CarrierCode = header.CarrierCode,
                ShipDate = header.ShipDate?.ToString("dd/MM/yyyy"),
                LcNumber = header.LcNumber,
                LcDate = header.LcDate?.ToString("dd/MM/yyyy"),
                ConsigneeName = invoicePrintDetails.ConsigneeName,
                ConsigneeAddressLines = invoicePrintDetails.ConsigneeAddressLines,
                NotifyPartyName = invoicePrintDetails.NotifyPartyName,
                NotifyPartyAddressLines = invoicePrintDetails.NotifyPartyAddressLines,
                IssuingBankName = invoicePrintDetails.IssuingBankName,
                LoadPortDescription = invoicePrintDetails.LoadPortDescription,
                DestinationDescription = invoicePrintDetails.DestinationDescription,
                Remark1 = header.Remark1,
                Remark2 = header.Remark2,
                Remark3 = header.Remark3,
                Detail = line?.Detail,
                Sizes = sizes,
                CartonGroups = cartonGroups,
                StringGroups = stringGroups,
            };
        }
    }
}
