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

            // 🔒 CONCURRENCY FIX: Read WITH (UPDLOCK, HOLDLOCK) so the row is exclusively locked until the caller's
            // transaction commits. A plain FirstOrDefaultAsync only takes a fleeting shared lock; if the database has
            // READ_COMMITTED_SNAPSHOT enabled, two concurrent callers could both read the same LastAllocatedNumber
            // and allocate duplicate document numbers. This must always be called inside an ambient transaction.
            var sequence = await _apparelProDbContext.DocumentSequences
                .FromSqlInterpolated($@"SELECT * FROM DocumentSequences WITH (UPDLOCK, HOLDLOCK)
                    WHERE NoteType = {noteType}")
                .FirstOrDefaultAsync();

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
