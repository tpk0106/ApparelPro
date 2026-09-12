using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for CUSDEC I/II (legacy ie_cusd1-4, keyed by CusNo) - header +
    // item lines (each with its own tax sub-lines) + attached documents,
    // all saved together in one transaction, replace-the-child-sets-on-save
    // shape (same as LetterOfCreditService/CommercialInvoiceService).
    public class CustomsDeclarationService : ICustomsDeclarationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public CustomsDeclarationService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<CustomsDeclarationDetailServiceModel?> GetByCusNoAsync(string cusNo)
        {
            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null) return null;

            var lines = await _apparelProDbContext.CustomsDeclarationLines
                .AsNoTracking()
                .Where(l => l.CusNo == cusNo)
                .ToListAsync();

            var taxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .AsNoTracking()
                .Where(t => t.CusNo == cusNo)
                .ToListAsync();

            var documents = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .AsNoTracking()
                .Where(d => d.CusNo == cusNo)
                .ToListAsync();

            var lineModels = _mapper.Map<List<CustomsDeclarationLineServiceModel>>(lines);
            foreach (var lineModel in lineModels)
            {
                lineModel.Taxes = _mapper.Map<List<CustomsDeclarationLineTaxServiceModel>>(
                    taxes.Where(t => t.Item == lineModel.Item));
            }

            return new CustomsDeclarationDetailServiceModel
            {
                Header = _mapper.Map<CustomsDeclarationHeaderServiceModel>(header),
                Lines = lineModels,
                AttachedDocuments = _mapper.Map<List<CustomsDeclarationAttachedDocumentServiceModel>>(documents),
            };
        }

        public async Task<CustomsDeclarationDetailServiceModel> SaveAsync(SaveCustomsDeclarationServiceModel serviceModel)
        {
            var cusNo = serviceModel.Header.CusNo;
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null)
            {
                header = _mapper.Map<CustomsDeclarationHeader>(serviceModel.Header);
                _apparelProDbContext.CustomsDeclarationHeaders.Add(header);
            }
            else
            {
                _mapper.Map(serviceModel.Header, header);
            }

            var existingTaxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .Where(t => t.CusNo == cusNo)
                .ToListAsync();
            _apparelProDbContext.CustomsDeclarationLineTaxes.RemoveRange(existingTaxes);

            var existingLines = await _apparelProDbContext.CustomsDeclarationLines
                .Where(l => l.CusNo == cusNo)
                .ToListAsync();
            _apparelProDbContext.CustomsDeclarationLines.RemoveRange(existingLines);

            var existingDocuments = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .Where(d => d.CusNo == cusNo)
                .ToListAsync();
            _apparelProDbContext.CustomsDeclarationAttachedDocuments.RemoveRange(existingDocuments);

            await _apparelProDbContext.SaveChangesAsync();

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<CustomsDeclarationLine>(l);
                line.Id = 0;
                line.CusNo = cusNo;
                return line;
            }).ToList();
            await _apparelProDbContext.CustomsDeclarationLines.AddRangeAsync(newLines);

            var newTaxes = serviceModel.Lines.SelectMany(l => l.Taxes.Select(t =>
            {
                var tax = _mapper.Map<CustomsDeclarationLineTax>(t);
                tax.Id = 0;
                tax.CusNo = cusNo;
                tax.Item = l.Item;
                return tax;
            })).ToList();
            await _apparelProDbContext.CustomsDeclarationLineTaxes.AddRangeAsync(newTaxes);

            var newDocuments = serviceModel.AttachedDocuments.Select(d =>
            {
                var document = _mapper.Map<CustomsDeclarationAttachedDocument>(d);
                document.Id = 0;
                document.CusNo = cusNo;
                return document;
            }).ToList();
            await _apparelProDbContext.CustomsDeclarationAttachedDocuments.AddRangeAsync(newDocuments);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByCusNoAsync(cusNo))!;
        }

        public async Task<bool> DeleteAsync(string cusNo)
        {
            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null) return false;

            var taxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .Where(t => t.CusNo == cusNo).ToListAsync();
            var lines = await _apparelProDbContext.CustomsDeclarationLines
                .Where(l => l.CusNo == cusNo).ToListAsync();
            var documents = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .Where(d => d.CusNo == cusNo).ToListAsync();

            _apparelProDbContext.CustomsDeclarationLineTaxes.RemoveRange(taxes);
            _apparelProDbContext.CustomsDeclarationLines.RemoveRange(lines);
            _apparelProDbContext.CustomsDeclarationAttachedDocuments.RemoveRange(documents);
            _apparelProDbContext.CustomsDeclarationHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
