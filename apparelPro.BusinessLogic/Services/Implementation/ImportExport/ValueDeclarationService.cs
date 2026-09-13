using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the Value Declaration Form (Sri Lanka Customs 308A). Two
    // coexisting flows share one table (additive, by explicit user
    // instruction - see ValueDeclarationHeader's own comment):
    //  - standalone (Id-keyed, GetByIdAsync/SaveAsync/DeleteAsync/
    //    GetPrintDetailsAsync) - the original design.
    //  - invoice-scoped (GetByInvoiceNumberAsync/SaveForInvoiceAsync/
    //    DeleteByInvoiceNumberAsync/GetPrintDetailsByInvoiceNumberAsync) -
    //    one Value Declaration per Commercial Invoice, same shape as
    //    Certificate of Origin, added alongside without touching the first.
    public class ValueDeclarationService : IValueDeclarationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICommercialInvoiceService _commercialInvoiceService;

        public ValueDeclarationService(
            IMapper mapper, ApparelProDbContext apparelProDbContext, ICommercialInvoiceService commercialInvoiceService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _commercialInvoiceService = commercialInvoiceService;
        }

        public async Task<PaginationResult<ValueDeclarationHeaderServiceModel>> GetValueDeclarationsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ValueDeclarationHeader> query = _apparelProDbContext.ValueDeclarationHeaders.AsNoTracking();

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterQuery))
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(ValueDeclarationHeader));
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
                query = query.OrderByDescending(h => h.Id);
            }

            var pageEntities = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var pageServiceModels = _mapper.Map<List<ValueDeclarationHeaderServiceModel>>(pageEntities);

            return new PaginationResult<ValueDeclarationHeaderServiceModel>(
                pageSize, pageNumber, total, pageServiceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<ValueDeclarationDetailServiceModel?> GetByIdAsync(int id)
        {
            var header = await _apparelProDbContext.ValueDeclarationHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
            if (header == null) return null;
            return await BuildDetailAsync(header);
        }

        public async Task<ValueDeclarationDetailServiceModel?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.ValueDeclarationHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return null;
            return await BuildDetailAsync(header);
        }

        private async Task<ValueDeclarationDetailServiceModel> BuildDetailAsync(ValueDeclarationHeader header)
        {
            var lines = await _apparelProDbContext.ValueDeclarationLines
                .AsNoTracking()
                .Where(l => l.ValueDeclarationHeaderId == header.Id)
                .OrderBy(l => l.ItemNo)
                .ToListAsync();

            return new ValueDeclarationDetailServiceModel
            {
                Header = _mapper.Map<ValueDeclarationHeaderServiceModel>(header),
                Lines = _mapper.Map<List<ValueDeclarationLineServiceModel>>(lines),
            };
        }

        public async Task<ValueDeclarationDetailServiceModel> SaveAsync(SaveValueDeclarationServiceModel serviceModel)
        {
            ValueDeclarationHeader header;
            if (serviceModel.Header.Id > 0)
            {
                header = await _apparelProDbContext.ValueDeclarationHeaders.FirstAsync(h => h.Id == serviceModel.Header.Id);
            }
            else
            {
                header = new ValueDeclarationHeader();
                _apparelProDbContext.ValueDeclarationHeaders.Add(header);
            }
            return await SaveInternalAsync(serviceModel, header);
        }

        public async Task<ValueDeclarationDetailServiceModel> SaveForInvoiceAsync(SaveValueDeclarationServiceModel serviceModel)
        {
            var header = await _apparelProDbContext.ValueDeclarationHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == serviceModel.Header.InvoiceNumber);
            if (header == null)
            {
                header = new ValueDeclarationHeader();
                _apparelProDbContext.ValueDeclarationHeaders.Add(header);
            }
            return await SaveInternalAsync(serviceModel, header);
        }

        private async Task<ValueDeclarationDetailServiceModel> SaveInternalAsync(
            SaveValueDeclarationServiceModel serviceModel, ValueDeclarationHeader header)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            _mapper.Map(serviceModel.Header, header);
            await _apparelProDbContext.SaveChangesAsync();

            var existingLines = await _apparelProDbContext.ValueDeclarationLines
                .Where(l => l.ValueDeclarationHeaderId == header.Id)
                .ToListAsync();
            _apparelProDbContext.ValueDeclarationLines.RemoveRange(existingLines);

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<ValueDeclarationLine>(l);
                // Every save deletes and re-inserts the whole line set, so Id
                // must never carry the frontend's own React-list-key
                // placeholder into the INSERT.
                line.Id = 0;
                line.ValueDeclarationHeaderId = header.Id;
                return line;
            }).ToList();
            await _apparelProDbContext.ValueDeclarationLines.AddRangeAsync(newLines);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByIdAsync(header.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var header = await _apparelProDbContext.ValueDeclarationHeaders.FirstOrDefaultAsync(h => h.Id == id);
            if (header == null) return false;
            return await DeleteInternalAsync(header);
        }

        public async Task<bool> DeleteByInvoiceNumberAsync(string invoiceNumber)
        {
            var header = await _apparelProDbContext.ValueDeclarationHeaders
                .FirstOrDefaultAsync(h => h.InvoiceNumber == invoiceNumber);
            if (header == null) return false;
            return await DeleteInternalAsync(header);
        }

        private async Task<bool> DeleteInternalAsync(ValueDeclarationHeader header)
        {
            var lines = await _apparelProDbContext.ValueDeclarationLines
                .Where(l => l.ValueDeclarationHeaderId == header.Id)
                .ToListAsync();

            _apparelProDbContext.ValueDeclarationLines.RemoveRange(lines);
            _apparelProDbContext.ValueDeclarationHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ValueDeclarationPrintDetailsServiceModel?> GetPrintDetailsAsync(int id)
        {
            var detail = await GetByIdAsync(id);
            if (detail == null) return null;

            var companyAddress = await _apparelProDbContext.CompanyAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == detail.Header.ImporterCompanyAddressId);

            return await BuildPrintDetailsAsync(
                detail,
                importerCompanyName: companyAddress?.CompanyName ?? "",
                importerAddress1: companyAddress?.Address1 ?? "",
                importerAddress2: companyAddress?.Address2 ?? "",
                importerCityPostCodeCountry: FormatCityPostCodeCountry(companyAddress),
                exporterCompanyName: null, exporterAddress1: null, exporterAddress2: null, exporterCityPostCodeCountry: null,
                invoiceDate: detail.Header.InvoiceDate?.ToDateTime(TimeOnly.MinValue));
        }

        public async Task<ValueDeclarationPrintDetailsServiceModel?> GetPrintDetailsByInvoiceNumberAsync(string invoiceNumber)
        {
            var detail = await GetByInvoiceNumberAsync(invoiceNumber);
            if (detail == null) return null;

            var exporterCompanyAddress = detail.Header.CompanyAddressId == null ? null : await _apparelProDbContext.CompanyAddresses
                .AsNoTracking().FirstOrDefaultAsync(a => a.Id == detail.Header.CompanyAddressId);

            // Importer (box 20) is resolved from the linked Commercial
            // Invoice's own Consignee - same reuse pattern as Certificate of
            // Origin - rather than a separate CompanyAddress selection.
            var invoicePrintDetails = await _commercialInvoiceService.GetPrintDetailsAsync(invoiceNumber);

            return await BuildPrintDetailsAsync(
                detail,
                importerCompanyName: invoicePrintDetails?.ConsigneeName ?? "",
                importerAddress1: invoicePrintDetails?.ConsigneeAddressLines.ElementAtOrDefault(0) ?? "",
                importerAddress2: invoicePrintDetails?.ConsigneeAddressLines.ElementAtOrDefault(1) ?? "",
                importerCityPostCodeCountry: "",
                exporterCompanyName: exporterCompanyAddress?.CompanyName ?? "",
                exporterAddress1: exporterCompanyAddress?.Address1 ?? "",
                exporterAddress2: exporterCompanyAddress?.Address2 ?? "",
                exporterCityPostCodeCountry: FormatCityPostCodeCountry(exporterCompanyAddress),
                invoiceDate: invoicePrintDetails?.Header.InvoiceDate);
        }

        private static string FormatCityPostCodeCountry(CompanyAddress? companyAddress) =>
            companyAddress == null
                ? ""
                : string.Join(", ", new[] { companyAddress.City, companyAddress.PostCode, companyAddress.Country }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

        private async Task<ValueDeclarationPrintDetailsServiceModel> BuildPrintDetailsAsync(
            ValueDeclarationDetailServiceModel detail,
            string importerCompanyName, string importerAddress1, string importerAddress2, string importerCityPostCodeCountry,
            string? exporterCompanyName, string? exporterAddress1, string? exporterAddress2, string? exporterCityPostCodeCountry,
            DateTime? invoiceDate)
        {
            var currency = detail.Header.CurrencyCode == null ? null : await _apparelProDbContext.Currencies
                .AsNoTracking().FirstOrDefaultAsync(c => c.Code == detail.Header.CurrencyCode);
            var deliveryTerm = detail.Header.TermsOfDeliveryCode == null ? null : await _apparelProDbContext.Basis
                .AsNoTracking().FirstOrDefaultAsync(b => b.Code == detail.Header.TermsOfDeliveryCode);
            var paymentTerm = detail.Header.TermsOfPaymentCode == null ? null : await _apparelProDbContext.PaymentTerms
                .AsNoTracking().FirstOrDefaultAsync(p => p.Code == detail.Header.TermsOfPaymentCode);
            var port = detail.Header.PortOfShipmentCode == null ? null : await _apparelProDbContext.Destinations
                .AsNoTracking().FirstOrDefaultAsync(d => d.Code == detail.Header.PortOfShipmentCode);

            var countryCodes = detail.Lines.Select(l => l.CountryOfOriginCode).Where(c => c != null).Distinct().ToList();
            var countries = await _apparelProDbContext.Countries
                .AsNoTracking().Where(c => countryCodes.Contains(c.Code)).ToListAsync();
            var unitCodes = detail.Lines.Select(l => l.UnitCode).Where(u => u != null).Distinct().ToList();
            var units = await _apparelProDbContext.Units
                .AsNoTracking().Where(u => unitCodes.Contains(u.Code)).ToListAsync();

            var linesWithDescriptions = detail.Lines.Select(l =>
            {
                var printLine = _mapper.Map<ValueDeclarationLinePrintServiceModel>(l);
                printLine.CountryOfOriginDescription = countries.FirstOrDefault(c => c.Code == l.CountryOfOriginCode)?.Name ?? l.CountryOfOriginCode ?? "";
                printLine.UnitDescription = l.UnitCode ?? "";
                return printLine;
            }).ToList();

            return new ValueDeclarationPrintDetailsServiceModel
            {
                Header = detail.Header,
                Lines = linesWithDescriptions,
                ImporterCompanyName = importerCompanyName,
                ImporterAddress1 = importerAddress1,
                ImporterAddress2 = importerAddress2,
                ImporterCityPostCodeCountry = importerCityPostCodeCountry,
                ExporterCompanyName = exporterCompanyName,
                ExporterAddress1 = exporterAddress1,
                ExporterAddress2 = exporterAddress2,
                ExporterCityPostCodeCountry = exporterCityPostCodeCountry,
                InvoiceDate = invoiceDate,
                CurrencyDescription = currency == null ? (detail.Header.CurrencyCode ?? "") : $"{currency.Code} - {currency.Name}",
                TermsOfDeliveryDescription = deliveryTerm?.Description ?? detail.Header.TermsOfDeliveryCode ?? "",
                TermsOfPaymentDescription = paymentTerm?.Description ?? detail.Header.TermsOfPaymentCode ?? "",
                PortOfShipmentDescription = port?.DestinationName ?? detail.Header.PortOfShipmentCode ?? "",
            };
        }
    }
}
