using apparelPro.BusinessLogic.Services.Models.ImportExport.ICompanyAddressService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // Legacy ie_setup holds multiple numbered addresses (confirmed against
    // production data), not a singleton - plain list CRUD keyed by Id, with
    // AddressNo carrying the legacy display order that other Import/Export
    // documents (e.g. Certificate of Origin) reference.
    public class CompanyAddressService : ICompanyAddressService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public CompanyAddressService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<List<CompanyAddressServiceModel>> GetAllAsync()
        {
            var entities = await _apparelProDbContext.CompanyAddresses
                .AsNoTracking()
                .OrderBy(e => e.AddressNo)
                .ToListAsync();
            return _mapper.Map<List<CompanyAddressServiceModel>>(entities);
        }

        public async Task<CompanyAddressServiceModel> SaveAsync(SaveCompanyAddressServiceModel serviceModel)
        {
            CompanyAddress entity;
            if (serviceModel.Id > 0)
            {
                entity = await _apparelProDbContext.CompanyAddresses.FirstAsync(e => e.Id == serviceModel.Id);
            }
            else
            {
                var maxAddressNo = await _apparelProDbContext.CompanyAddresses
                    .Select(e => (int?)e.AddressNo)
                    .MaxAsync();
                entity = new CompanyAddress { AddressNo = (maxAddressNo ?? 0) + 1 };
                _apparelProDbContext.CompanyAddresses.Add(entity);
            }

            entity.CompanyName = serviceModel.CompanyName;
            entity.Address1 = serviceModel.Address1;
            entity.Address2 = serviceModel.Address2;
            entity.City = serviceModel.City;
            entity.PostCode = serviceModel.PostCode;
            entity.Country = serviceModel.Country;
            entity.TelNos = serviceModel.TelNos;
            entity.FaxNos = serviceModel.FaxNos;
            entity.TinNo = serviceModel.TinNo;
            entity.ExportRegNo = serviceModel.ExportRegNo;

            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<CompanyAddressServiceModel>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _apparelProDbContext.CompanyAddresses.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null) return false;

            _apparelProDbContext.CompanyAddresses.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
