using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class UnitServiceT : IUnitServiceT<UnitServiceModel>
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;

        public UnitServiceT(IMapper mapper, ApparelProDbContext apparelProDbContext,
            ILookupConstants lookupConstants)
        {
            if (apparelProDbContext == null)
            {
                throw new ArgumentNullException(nameof(apparelProDbContext));
            }
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (lookupConstants == null)
            {
                throw new ArgumentNullException(nameof(lookupConstants));
            }
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
        }
        public async Task<UnitServiceModel> AddUnitAsync(CreateUnitServiceModel createUnitServiceModel)
        {
            var unitDbModel = _mapper.Map<Unit>(createUnitServiceModel);
            _apparelProDbContext.Units.Add(unitDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<UnitServiceModel>(unitDbModel);
        }

        public async Task DeleteUnitAsync(string code)
        {
            var unitDbModel = await _apparelProDbContext.Units
               .Where(unit => unit.Code == code)
               .FirstOrDefaultAsync();
            _apparelProDbContext.Units.Remove(unitDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<UnitServiceModel> GetUnitByCodeAsync(string code)
        {
            var unitDbModel = await _apparelProDbContext.Units
               .Where(unit => unit.Code == code)
               .AsNoTracking()
               .FirstOrDefaultAsync();
            var unitServiceModel = _mapper.Map<UnitServiceModel>(unitDbModel);
            return unitServiceModel;
        }

        public async Task<IEnumerable<UnitServiceModel>> GetUnitsAsync()
        {
            var unitDbModels = await _apparelProDbContext.Units.ToListAsync();
            var unitServiceModels = _mapper.Map<IEnumerable<UnitServiceModel>>(unitDbModels);
            return unitServiceModels;
        }

        public Task UpdateUnitAsync(UpdateUnitServiceModel updateUnitServiceModel)
        {
            throw new NotImplementedException();
        }
    }
}
