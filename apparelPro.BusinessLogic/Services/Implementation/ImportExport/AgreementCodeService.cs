using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IAgreementCodeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class AgreementCodeService : IAgreementCodeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public AgreementCodeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<AgreementCodeServiceModel> AddAgreementCodeAsync(CreateAgreementCodeServiceModel createAgreementCodeServiceModel)
        {
            try
            {
                var agreementCodeDbModel = _mapper.Map<AgreementCode>(createAgreementCodeServiceModel);
                _apparelProDbContext.AgreementCodes.Add(agreementCodeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<AgreementCodeServiceModel>(agreementCodeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Agreement Code already exists");
            }
        }

        public async Task DeleteAgreementCodeAsync(string code)
        {
            try
            {
                var agreementCodeDbModel = await _apparelProDbContext.AgreementCodes
                    .Where(agreementCode => agreementCode.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.AgreementCodes.Remove(agreementCodeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<AgreementCodeServiceModel>> GetAgreementCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<AgreementCode> agreementCodePagination = _apparelProDbContext.AgreementCodes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(AgreementCode));
                agreementCodePagination = agreementCodePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await agreementCodePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                agreementCodePagination = agreementCodePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            agreementCodePagination = agreementCodePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbAgreementCodes = await agreementCodePagination.ToListAsync();
            var agreementCodeServiceModels = _mapper.Map<IList<AgreementCodeServiceModel>>(filteredDbAgreementCodes);

            return new PaginationResult<AgreementCodeServiceModel>(pageSize, pageNumber, counter, agreementCodeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateAgreementCodeAsync(UpdateAgreementCodeServiceModel updateAgreementCodeServiceModel)
        {
            try
            {
                var agreementCodeDbModel = await _apparelProDbContext.AgreementCodes
                    .Where(agreementCode => agreementCode.Code == updateAgreementCodeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (agreementCodeDbModel != null)
                {
                    agreementCodeDbModel.Description = updateAgreementCodeServiceModel.Description;
                    _apparelProDbContext.AgreementCodes.Update(agreementCodeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<AgreementCodeServiceModel> GetAgreementCodeByCodeAsync(string code)
        {
            var agreementCodeDbModel = await _apparelProDbContext.AgreementCodes.Where(agreementCode => agreementCode.Code == code)
                .FirstOrDefaultAsync();
            var agreementCodeServiceModel = _mapper.Map<AgreementCodeServiceModel>(agreementCodeDbModel);
            return agreementCodeServiceModel;
        }
    }
}
