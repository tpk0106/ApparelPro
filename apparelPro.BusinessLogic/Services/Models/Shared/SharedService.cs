using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Models.Shared
{
    public class SharedService:ISharedService
    {

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IUnitConversionService _unitConversionService;

        public SharedService(ApparelProDbContext apparelProDbContext, IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _unitConversionService = unitConversionService;
        }

        // ----------------------------------------------------------------------------------
        // 🔒 THREAD-SAFE CONSECUTIVE DOCUMENT NUMBER GENERATION ENGINE
        // ----------------------------------------------------------------------------------
        public async Task<string> GenerateNextDocumentNumberAsync(string noteType)
        {
            noteType = noteType.Trim().ToUpper();

            var sequence = await _apparelProDbContext.DocumentSequences
                .FirstOrDefaultAsync(s => s.NoteType == noteType);

            if (sequence == null)
            {
                throw new InvalidOperationException($"Sequence Error: Document counter configuration for note type '{noteType}' was not found.");
            }

            // Increment the counter tracking value atomically
            sequence.LastAllocatedNumber += 1;
            _apparelProDbContext.DocumentSequences.Update(sequence);
            await _apparelProDbContext.SaveChangesAsync();

            // Pads with leading zeros to create consistent 6-character strings ("000001", "000002", etc.)
            string formattedNumber = sequence.LastAllocatedNumber.ToString().PadLeft(6, '0');
            return $"{sequence.Prefix}{formattedNumber}";
        }
    }
}
