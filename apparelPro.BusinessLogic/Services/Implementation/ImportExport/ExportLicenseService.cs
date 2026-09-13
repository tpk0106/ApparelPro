using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the Export Control License application (Department of
    // Imports & Exports Control) - a standalone document (own list, not
    // tied to CommercialInvoiceHeader; the real form has no Invoice No.
    // field). Header + line items saved together in one transaction, same
    // replace-the-line-set-on-save shape as Value Declaration/Certificate
    // of Origin.
    public class ExportLicenseService : IExportLicenseService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public ExportLicenseService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<ExportLicenseHeaderServiceModel>> GetExportLicensesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ExportLicenseHeader> query = _apparelProDbContext.ExportLicenseHeaders.AsNoTracking();

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterQuery))
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(ExportLicenseHeader));
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
            var pageServiceModels = _mapper.Map<List<ExportLicenseHeaderServiceModel>>(pageEntities);

            return new PaginationResult<ExportLicenseHeaderServiceModel>(
                pageSize, pageNumber, total, pageServiceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<ExportLicenseDetailServiceModel?> GetByIdAsync(int id)
        {
            var header = await _apparelProDbContext.ExportLicenseHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
            if (header == null) return null;

            var lines = await _apparelProDbContext.ExportLicenseLines
                .AsNoTracking()
                .Where(l => l.ExportLicenseHeaderId == id)
                .OrderBy(l => l.ItemNo)
                .ToListAsync();

            return new ExportLicenseDetailServiceModel
            {
                Header = _mapper.Map<ExportLicenseHeaderServiceModel>(header),
                Lines = _mapper.Map<List<ExportLicenseLineServiceModel>>(lines),
            };
        }

        public async Task<ExportLicenseDetailServiceModel> SaveAsync(SaveExportLicenseServiceModel serviceModel)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            ExportLicenseHeader header;
            if (serviceModel.Header.Id > 0)
            {
                header = await _apparelProDbContext.ExportLicenseHeaders.FirstAsync(h => h.Id == serviceModel.Header.Id);
                _mapper.Map(serviceModel.Header, header);
            }
            else
            {
                header = _mapper.Map<ExportLicenseHeader>(serviceModel.Header);
                _apparelProDbContext.ExportLicenseHeaders.Add(header);
            }
            await _apparelProDbContext.SaveChangesAsync();

            var existingLines = await _apparelProDbContext.ExportLicenseLines
                .Where(l => l.ExportLicenseHeaderId == header.Id)
                .ToListAsync();
            _apparelProDbContext.ExportLicenseLines.RemoveRange(existingLines);

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<ExportLicenseLine>(l);
                // Every save deletes and re-inserts the whole line set, so Id
                // must never carry the frontend's own React-list-key
                // placeholder into the INSERT.
                line.Id = 0;
                line.ExportLicenseHeaderId = header.Id;
                return line;
            }).ToList();
            await _apparelProDbContext.ExportLicenseLines.AddRangeAsync(newLines);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByIdAsync(header.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var header = await _apparelProDbContext.ExportLicenseHeaders.FirstOrDefaultAsync(h => h.Id == id);
            if (header == null) return false;

            var lines = await _apparelProDbContext.ExportLicenseLines
                .Where(l => l.ExportLicenseHeaderId == id)
                .ToListAsync();

            _apparelProDbContext.ExportLicenseLines.RemoveRange(lines);
            _apparelProDbContext.ExportLicenseHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ExportLicensePrintDetailsServiceModel?> GetPrintDetailsAsync(int id)
        {
            var detail = await GetByIdAsync(id);
            if (detail == null) return null;

            var companyAddress = await _apparelProDbContext.CompanyAddresses
                .AsNoTracking().FirstOrDefaultAsync(a => a.Id == detail.Header.CompanyAddressId);
            var bank = detail.Header.BankCode == null ? null : await _apparelProDbContext.Banks
                .AsNoTracking().FirstOrDefaultAsync(b => b.BankCode == detail.Header.BankCode);

            async Task<(string Name, string Address)> ResolveConsigneeAsync(int? buyerCode)
            {
                if (buyerCode == null) return ("", "");
                var buyer = await _apparelProDbContext.Buyers.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);
                if (buyer == null) return ("", "");
                var address = await _apparelProDbContext.Addresses.AsNoTracking()
                    .Where(a => a.BuyerCode == buyerCode)
                    .OrderByDescending(a => a.Default == true)
                    .FirstOrDefaultAsync();
                var addressLine = address == null
                    ? ""
                    : string.Join(", ", new[] { address.StreetAddress, address.City, address.State }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                return (buyer.Name, addressLine);
            }

            var (consignee1Name, consignee1Address) = await ResolveConsigneeAsync(detail.Header.Consignee1BuyerCode);
            var (consignee2Name, consignee2Address) = await ResolveConsigneeAsync(detail.Header.Consignee2BuyerCode);

            return new ExportLicensePrintDetailsServiceModel
            {
                Header = detail.Header,
                Lines = detail.Lines,
                ApplicantCompanyName = companyAddress?.CompanyName ?? "",
                ApplicantAddress1 = companyAddress?.Address1 ?? "",
                ApplicantAddress2 = companyAddress?.Address2 ?? "",
                ApplicantCityPostCodeCountry = companyAddress == null
                    ? ""
                    : string.Join(", ", new[] { companyAddress.City, companyAddress.PostCode, companyAddress.Country }
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                BankName = bank?.Name ?? detail.Header.BankCode ?? "",
                Consignee1Name = consignee1Name,
                Consignee1Address = consignee1Address,
                Consignee2Name = consignee2Name,
                Consignee2Address = consignee2Address,
            };
        }
    }
}
