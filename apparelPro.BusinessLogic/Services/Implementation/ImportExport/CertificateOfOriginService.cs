using apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the Certificate of Origin (EXP 5, Ceylon Chamber of Commerce
    // form) - header + its line items saved together in one transaction,
    // same replace-the-line-set-on-save shape as CommercialInvoiceService.
    public class CertificateOfOriginService : ICertificateOfOriginService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICommercialInvoiceService _commercialInvoiceService;

        public CertificateOfOriginService(
            IMapper mapper, ApparelProDbContext apparelProDbContext, ICommercialInvoiceService commercialInvoiceService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _commercialInvoiceService = commercialInvoiceService;
        }

        public async Task<CertificateOfOriginDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.CertificateOfOriginHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return null;

            var lines = await _apparelProDbContext.CertificateOfOriginLines
                .AsNoTracking()
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .OrderBy(l => l.ItemNo)
                .ToListAsync();

            return new CertificateOfOriginDetailServiceModel
            {
                Header = _mapper.Map<CertificateOfOriginHeaderServiceModel>(header),
                Lines = _mapper.Map<List<CertificateOfOriginLineServiceModel>>(lines),
            };
        }

        public async Task<CertificateOfOriginDetailServiceModel> SaveAsync(SaveCertificateOfOriginServiceModel serviceModel)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var header = await _apparelProDbContext.CertificateOfOriginHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == serviceModel.Header.InvoiceNumber);
            if (header == null)
            {
                header = _mapper.Map<CertificateOfOriginHeader>(serviceModel.Header);
                _apparelProDbContext.CertificateOfOriginHeaders.Add(header);
            }
            else
            {
                _mapper.Map(serviceModel.Header, header);
            }

            var existingLines = await _apparelProDbContext.CertificateOfOriginLines
                .Where(l => l.InvoiceNumber == serviceModel.Header.InvoiceNumber)
                .ToListAsync();
            _apparelProDbContext.CertificateOfOriginLines.RemoveRange(existingLines);

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<CertificateOfOriginLine>(l);
                // Every save deletes and re-inserts the whole line set (same
                // as CommercialInvoiceService), so Id must never carry the
                // frontend's own React-list-key placeholder into the INSERT.
                line.Id = 0;
                line.InvoiceNumber = serviceModel.Header.InvoiceNumber;
                return line;
            }).ToList();
            await _apparelProDbContext.CertificateOfOriginLines.AddRangeAsync(newLines);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByInvoiceNumberAsync(serviceModel.Header.InvoiceNumber))!;
        }

        public async Task<bool> DeleteAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.CertificateOfOriginHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return false;

            var lines = await _apparelProDbContext.CertificateOfOriginLines
                .Where(l => l.InvoiceNumber == invoiceNumber)
                .ToListAsync();

            _apparelProDbContext.CertificateOfOriginLines.RemoveRange(lines);
            _apparelProDbContext.CertificateOfOriginHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<CertificateOfOriginPrintDetailsServiceModel?> GetPrintDetailsAsync(string invoiceNumber)
        {
            var detail = await GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return null;

            var companyAddress = await _apparelProDbContext.CompanyAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == detail.Header.CompanyAddressId);

            // Reuses Commercial Invoice's own Consignee resolution (Buyer
            // fallback when no distinct Consignee code is set) rather than
            // duplicating that logic here - the certificate is always
            // invoice-scoped, so this is exactly the same consignee.
            var invoicePrintDetails = await _commercialInvoiceService.GetPrintDetailsAsync(invoiceNumber);

            return new CertificateOfOriginPrintDetailsServiceModel
            {
                Header = detail.Header,
                Lines = detail.Lines,
                ExporterCompanyName = companyAddress?.CompanyName ?? "",
                ExporterAddress1 = companyAddress?.Address1 ?? "",
                ExporterAddress2 = companyAddress?.Address2 ?? "",
                ExporterCityPostCodeCountry = companyAddress == null
                    ? ""
                    : string.Join(", ", new[] { companyAddress.City, companyAddress.PostCode, companyAddress.Country }
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                ConsigneeName = invoicePrintDetails?.ConsigneeName ?? "",
                ConsigneeAddressLines = invoicePrintDetails?.ConsigneeAddressLines ?? new List<string>(),
            };
        }
    }
}
