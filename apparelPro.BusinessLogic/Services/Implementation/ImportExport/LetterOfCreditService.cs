using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the Letter of Credit form (legacy ie_lc/ie_lc2, keyed by
    // BankCode+LcNo, not an invoice number) - header + its line items saved
    // together in one transaction, same replace-the-line-set-on-save shape
    // as CommercialInvoiceService/CertificateOfOriginService.
    public class LetterOfCreditService : ILetterOfCreditService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public LetterOfCreditService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<LetterOfCreditDetailServiceModel?> GetByKeyAsync(string bankCode, string lcNo)
        {
            var header = await _apparelProDbContext.LetterOfCreditHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.BankCode == bankCode && h.LcNo == lcNo);
            if (header == null) return null;

            var lines = await _apparelProDbContext.LetterOfCreditLines
                .AsNoTracking()
                .Where(l => l.BankCode == bankCode && l.LcNo == lcNo)
                .ToListAsync();

            return new LetterOfCreditDetailServiceModel
            {
                Header = _mapper.Map<LetterOfCreditHeaderServiceModel>(header),
                Lines = _mapper.Map<List<LetterOfCreditLineServiceModel>>(lines),
            };
        }

        public async Task<LetterOfCreditDetailServiceModel> SaveAsync(SaveLetterOfCreditServiceModel serviceModel)
        {
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var header = await _apparelProDbContext.LetterOfCreditHeaders
                .FirstOrDefaultAsync(h => h.BankCode == serviceModel.Header.BankCode && h.LcNo == serviceModel.Header.LcNo);
            if (header == null)
            {
                header = _mapper.Map<LetterOfCreditHeader>(serviceModel.Header);
                _apparelProDbContext.LetterOfCreditHeaders.Add(header);
            }
            else
            {
                _mapper.Map(serviceModel.Header, header);
            }

            var existingLines = await _apparelProDbContext.LetterOfCreditLines
                .Where(l => l.BankCode == serviceModel.Header.BankCode && l.LcNo == serviceModel.Header.LcNo)
                .ToListAsync();
            _apparelProDbContext.LetterOfCreditLines.RemoveRange(existingLines);

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<LetterOfCreditLine>(l);
                line.Id = 0;
                line.BankCode = serviceModel.Header.BankCode;
                line.LcNo = serviceModel.Header.LcNo;
                return line;
            }).ToList();
            await _apparelProDbContext.LetterOfCreditLines.AddRangeAsync(newLines);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByKeyAsync(serviceModel.Header.BankCode, serviceModel.Header.LcNo))!;
        }

        public async Task<bool> DeleteAsync(string bankCode, string lcNo)
        {
            var header = await _apparelProDbContext.LetterOfCreditHeaders
                .FirstOrDefaultAsync(h => h.BankCode == bankCode && h.LcNo == lcNo);
            if (header == null) return false;

            var lines = await _apparelProDbContext.LetterOfCreditLines
                .Where(l => l.BankCode == bankCode && l.LcNo == lcNo)
                .ToListAsync();

            _apparelProDbContext.LetterOfCreditLines.RemoveRange(lines);
            _apparelProDbContext.LetterOfCreditHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
