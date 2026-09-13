using apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using apparelPro.BusinessLogic.Services;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the Boat Note (Sri Lanka Customs e-CDN "goods passed out of
    // Customs control" release document) - header + cargo lines saved
    // together in one transaction, same replace-the-line-set-on-save shape
    // as Certificate of Origin/Commercial Invoice.
    public class BoatNoteService : IBoatNoteService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICommercialInvoiceService _commercialInvoiceService;

        public BoatNoteService(
            IMapper mapper, ApparelProDbContext apparelProDbContext, ICommercialInvoiceService commercialInvoiceService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _commercialInvoiceService = commercialInvoiceService;
        }

        public async Task<BoatNoteDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.BoatNoteHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return null;

            var lines = await _apparelProDbContext.BoatNoteCargoLines
                .AsNoTracking()
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .OrderBy(l => l.LineNo)
                .ToListAsync();

            return new BoatNoteDetailServiceModel
            {
                Header = _mapper.Map<BoatNoteHeaderServiceModel>(header),
                Lines = _mapper.Map<List<BoatNoteCargoLineServiceModel>>(lines),
            };
        }

        public async Task<BoatNoteDetailServiceModel> SaveAsync(SaveBoatNoteServiceModel serviceModel)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var header = await _apparelProDbContext.BoatNoteHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == serviceModel.Header.InvoiceNumber);
            if (header == null)
            {
                header = _mapper.Map<BoatNoteHeader>(serviceModel.Header);
                _apparelProDbContext.BoatNoteHeaders.Add(header);
            }
            else
            {
                _mapper.Map(serviceModel.Header, header);
            }

            var existingLines = await _apparelProDbContext.BoatNoteCargoLines
                .Where(l => l.InvoiceNumber == serviceModel.Header.InvoiceNumber)
                .ToListAsync();
            _apparelProDbContext.BoatNoteCargoLines.RemoveRange(existingLines);

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<BoatNoteCargoLine>(l);
                // Every save deletes and re-inserts the whole line set (same
                // as Certificate of Origin), so Id must never carry the
                // frontend's own React-list-key placeholder into the INSERT.
                line.Id = 0;
                line.InvoiceNumber = serviceModel.Header.InvoiceNumber;
                return line;
            }).ToList();
            await _apparelProDbContext.BoatNoteCargoLines.AddRangeAsync(newLines);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByInvoiceNumberAsync(serviceModel.Header.InvoiceNumber))!;
        }

        public async Task<bool> DeleteAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.BoatNoteHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return false;

            var lines = await _apparelProDbContext.BoatNoteCargoLines
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .ToListAsync();

            _apparelProDbContext.BoatNoteCargoLines.RemoveRange(lines);
            _apparelProDbContext.BoatNoteHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<BoatNotePrintDetailsServiceModel?> GetPrintDetailsAsync(string invoiceNumber)
        {
            var detail = await GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return null;

            var companyAddress = await _apparelProDbContext.CompanyAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == detail.Header.CompanyAddressId);

            // Reuses Commercial Invoice's own Consignee resolution (Buyer
            // fallback when no distinct Consignee code is set) rather than
            // duplicating that logic here - the Boat Note is always
            // invoice-scoped, so this is exactly the same consignee.
            var invoicePrintDetails = await _commercialInvoiceService.GetPrintDetailsAsync(invoiceNumber);

            var loadPort = detail.Header.PortOfLoadingCode == null ? null : await _apparelProDbContext.Destinations
                .AsNoTracking().FirstOrDefaultAsync(d => d.Code == detail.Header.PortOfLoadingCode);
            var dischargePort = detail.Header.DischargePortCode == null ? null : await _apparelProDbContext.Destinations
                .AsNoTracking().FirstOrDefaultAsync(d => d.Code == detail.Header.DischargePortCode);

            return new BoatNotePrintDetailsServiceModel
            {
                Header = detail.Header,
                Lines = detail.Lines,
                ShipperName = companyAddress?.CompanyName ?? "",
                ConsigneeName = invoicePrintDetails?.ConsigneeName ?? "",
                ConsigneeAddressLines = invoicePrintDetails?.ConsigneeAddressLines ?? new List<string>(),
                PortOfLoadingDescription = loadPort?.DestinationName ?? detail.Header.PortOfLoadingCode ?? "",
                DischargePortDescription = dischargePort?.DestinationName ?? detail.Header.DischargePortCode ?? "",
            };
        }
    }
}
