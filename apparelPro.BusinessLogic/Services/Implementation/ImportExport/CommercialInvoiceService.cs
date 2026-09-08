using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the export Commercial Invoice (legacy ie_coinv/ie_coin2) -
    // header + its line items saved together in one transaction, same
    // replace-the-line-set-on-save shape as ToolbarService.SavePreferencesAsync.
    public class CommercialInvoiceService : ICommercialInvoiceService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public CommercialInvoiceService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<CommercialInvoiceHeaderServiceModel>> GetInvoicesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<CommercialInvoiceHeader> query = _apparelProDbContext.CommercialInvoiceHeaders.AsNoTracking();

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterQuery))
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(CommercialInvoiceHeader));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var total = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy($"{sortColumn} {sortOrder}");
            }
            else
            {
                query = query.OrderByDescending(h => h.InvoiceDate);
            }

            var pageEntities = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var pageServiceModels = _mapper.Map<List<CommercialInvoiceHeaderServiceModel>>(pageEntities);

            return new PaginationResult<CommercialInvoiceHeaderServiceModel>(
                pageSize, pageNumber, total, pageServiceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<CommercialInvoiceDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.CommercialInvoiceHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return null;

            var lines = await _apparelProDbContext.CommercialInvoiceLines
                .AsNoTracking()
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .ToListAsync();

            return new CommercialInvoiceDetailServiceModel
            {
                Header = _mapper.Map<CommercialInvoiceHeaderServiceModel>(header),
                Lines = _mapper.Map<List<CommercialInvoiceLineServiceModel>>(lines),
            };
        }

        public async Task<CommercialInvoiceDetailServiceModel> SaveAsync(SaveCommercialInvoiceServiceModel serviceModel)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var header = await _apparelProDbContext.CommercialInvoiceHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == serviceModel.Header.InvoiceNumber);
            if (header == null)
            {
                header = _mapper.Map<CommercialInvoiceHeader>(serviceModel.Header);
                _apparelProDbContext.CommercialInvoiceHeaders.Add(header);
            }
            else
            {
                _mapper.Map(serviceModel.Header, header);
            }

            var existingLines = await _apparelProDbContext.CommercialInvoiceLines
                .Where(l => l.InvoiceNumber == serviceModel.Header.InvoiceNumber)
                .ToListAsync();
            _apparelProDbContext.CommercialInvoiceLines.RemoveRange(existingLines);

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<CommercialInvoiceLine>(l);
                // Every save deletes and re-inserts the whole line set, so
                // Id is always a fresh identity value - the frontend's own
                // Id is just a React list key (negative placeholder for a
                // brand-new row) and must never reach the INSERT, or SQL
                // Server rejects it as an explicit identity-column value.
                line.Id = 0;
                line.InvoiceNumber = serviceModel.Header.InvoiceNumber;
                return line;
            }).ToList();
            await _apparelProDbContext.CommercialInvoiceLines.AddRangeAsync(newLines);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByInvoiceNumberAsync(serviceModel.Header.InvoiceNumber))!;
        }

        public async Task<bool> DeleteAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.CommercialInvoiceHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return false;

            var lines = await _apparelProDbContext.CommercialInvoiceLines
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .ToListAsync();

            _apparelProDbContext.CommercialInvoiceLines.RemoveRange(lines);
            _apparelProDbContext.CommercialInvoiceHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<CommercialInvoicePrintDetailsServiceModel?> GetPrintDetailsAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.CommercialInvoiceHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return null;

            var companySetup = await _apparelProDbContext.CompanyAddresses
                .AsNoTracking().OrderBy(c => c.AddressNo).FirstOrDefaultAsync();

            var buyer = await _apparelProDbContext.Buyers.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == header.BuyerCode);

            // The invoice entry form doesn't capture a separate Consignee code
            // yet (see project_commercial_invoice_print_extra_fields_todo) - in
            // practice the Consignee is the invoice's own Buyer, so fall back to
            // the Buyer's own address when no distinct Consignee code is set.
            int.TryParse(header.ConsigneeCode, out var parsedConsigneeCode);
            var consigneeCode = parsedConsigneeCode > 0 ? parsedConsigneeCode : header.BuyerCode;
            var consignee = parsedConsigneeCode > 0
                ? await _apparelProDbContext.Buyers.AsNoTracking().FirstOrDefaultAsync(b => b.BuyerCode == consigneeCode)
                : buyer;
            var consigneeAddresses = consigneeCode > 0
                ? await _apparelProDbContext.Addresses.AsNoTracking().Where(a => a.BuyerCode == consigneeCode).ToListAsync()
                : new List<Address>();

            int.TryParse(header.NotifyPartyCode, out var notifyCode);
            var notifyParty = notifyCode > 0
                ? await _apparelProDbContext.Buyers.AsNoTracking().FirstOrDefaultAsync(b => b.BuyerCode == notifyCode)
                : null;
            var notifyAddresses = notifyCode > 0
                ? await _apparelProDbContext.Addresses.AsNoTracking().Where(a => a.BuyerCode == notifyCode).ToListAsync()
                : new List<Address>();

            var loadPort = header.LoadPortCode == null ? null : await _apparelProDbContext.Destinations
                .AsNoTracking().FirstOrDefaultAsync(d => d.Code == header.LoadPortCode);
            var destination = header.DestinationCode == null ? null : await _apparelProDbContext.Destinations
                .AsNoTracking().FirstOrDefaultAsync(d => d.Code == header.DestinationCode);

            var bank = header.IssuingBankCode == null ? null : await _apparelProDbContext.Banks
                .AsNoTracking().FirstOrDefaultAsync(b => b.BankCode == header.IssuingBankCode);

            static List<string> AddressLines(List<Address> addresses) =>
                addresses
                    .OrderByDescending(a => a.Default == true)
                    .Take(1)
                    .SelectMany(a => new[] { a.StreetAddress, a.City, a.State }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    .Select(s => s!)
                    .ToList();

            var lines = await _apparelProDbContext.CommercialInvoiceLines
                .AsNoTracking()
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .ToListAsync();

            return new CommercialInvoicePrintDetailsServiceModel
            {
                Header = _mapper.Map<CommercialInvoiceHeaderServiceModel>(header),
                Lines = _mapper.Map<List<CommercialInvoiceLineServiceModel>>(lines),
                ShipperCompanyName = companySetup?.CompanyName ?? "",
                ShipperAddress1 = companySetup?.Address1 ?? "",
                ShipperAddress2 = companySetup?.Address2 ?? "",
                ShipperAddress3 = companySetup == null
                    ? ""
                    : string.Join(", ", new[] { companySetup.City, companySetup.PostCode, companySetup.Country }
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                BuyerName = buyer?.Name ?? "",
                ConsigneeName = consignee?.Name ?? "",
                ConsigneeAddressLines = AddressLines(consigneeAddresses),
                NotifyPartyName = notifyParty?.Name ?? "",
                NotifyPartyAddressLines = AddressLines(notifyAddresses),
                LoadPortDescription = loadPort?.DestinationName ?? header.LoadPortCode ?? "",
                DestinationDescription = destination?.DestinationName ?? header.DestinationCode ?? "",
                IssuingBankName = bank?.Name ?? header.IssuingBankCode ?? "",
            };
        }
    }
}
