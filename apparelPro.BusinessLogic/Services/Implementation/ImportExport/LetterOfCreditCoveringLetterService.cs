using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditCoveringLetterService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for the L/C Covering Letter (legacy ie_lclet, IE_LCLT1.PRG) - a
    // single-record form per BankCode+LcNo, no line items, same get-or-create
    // on save shape as ToolbarPreference/CompanyAddress in this codebase.
    public class LetterOfCreditCoveringLetterService : ILetterOfCreditCoveringLetterService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public LetterOfCreditCoveringLetterService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<LetterOfCreditCoveringLetterServiceModel?> GetByKeyAsync(string bankCode, string lcNo)
        {
            var entity = await _apparelProDbContext.LetterOfCreditCoveringLetters
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.BankCode == bankCode && e.LcNo == lcNo);
            return entity == null ? null : _mapper.Map<LetterOfCreditCoveringLetterServiceModel>(entity);
        }

        public async Task<LetterOfCreditCoveringLetterServiceModel> SaveAsync(LetterOfCreditCoveringLetterServiceModel serviceModel)
        {
            var entity = await _apparelProDbContext.LetterOfCreditCoveringLetters
                .FirstOrDefaultAsync(e => e.BankCode == serviceModel.BankCode && e.LcNo == serviceModel.LcNo);
            if (entity == null)
            {
                entity = _mapper.Map<LetterOfCreditCoveringLetter>(serviceModel);
                _apparelProDbContext.LetterOfCreditCoveringLetters.Add(entity);
            }
            else
            {
                _mapper.Map(serviceModel, entity);
            }

            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<LetterOfCreditCoveringLetterServiceModel>(entity);
        }

        public async Task<bool> DeleteAsync(string bankCode, string lcNo)
        {
            var entity = await _apparelProDbContext.LetterOfCreditCoveringLetters
                .FirstOrDefaultAsync(e => e.BankCode == bankCode && e.LcNo == lcNo);
            if (entity == null) return false;

            _apparelProDbContext.LetterOfCreditCoveringLetters.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
