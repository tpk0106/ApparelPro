using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IDutyTaxCodeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class DutyTaxCodeService : IDutyTaxCodeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public DutyTaxCodeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<DutyTaxCodeServiceModel> AddDutyTaxCodeAsync(CreateDutyTaxCodeServiceModel createDutyTaxCodeServiceModel)
        {
            try
            {
                var dutyTaxCodeDbModel = _mapper.Map<DutyTaxCode>(createDutyTaxCodeServiceModel);
                _apparelProDbContext.DutyTaxCodes.Add(dutyTaxCodeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<DutyTaxCodeServiceModel>(dutyTaxCodeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Duty Tax Code already exists");
            }
        }

        public async Task DeleteDutyTaxCodeAsync(string code)
        {
            try
            {
                var dutyTaxCodeDbModel = await _apparelProDbContext.DutyTaxCodes
                    .Where(dutyTaxCode => dutyTaxCode.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.DutyTaxCodes.Remove(dutyTaxCodeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<DutyTaxCodeServiceModel>> GetDutyTaxCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<DutyTaxCode> dutyTaxCodePagination = _apparelProDbContext.DutyTaxCodes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(DutyTaxCode));
                dutyTaxCodePagination = dutyTaxCodePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await dutyTaxCodePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                dutyTaxCodePagination = dutyTaxCodePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            dutyTaxCodePagination = dutyTaxCodePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbDutyTaxCodes = await dutyTaxCodePagination.ToListAsync();
            var dutyTaxCodeServiceModels = _mapper.Map<IList<DutyTaxCodeServiceModel>>(filteredDbDutyTaxCodes);

            return new PaginationResult<DutyTaxCodeServiceModel>(pageSize, pageNumber, counter, dutyTaxCodeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateDutyTaxCodeAsync(UpdateDutyTaxCodeServiceModel updateDutyTaxCodeServiceModel)
        {
            try
            {
                var dutyTaxCodeDbModel = await _apparelProDbContext.DutyTaxCodes
                    .Where(dutyTaxCode => dutyTaxCode.Code == updateDutyTaxCodeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (dutyTaxCodeDbModel != null)
                {
                    dutyTaxCodeDbModel.Description = updateDutyTaxCodeServiceModel.Description;
                    _apparelProDbContext.DutyTaxCodes.Update(dutyTaxCodeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<DutyTaxCodeServiceModel> GetDutyTaxCodeByCodeAsync(string code)
        {
            var dutyTaxCodeDbModel = await _apparelProDbContext.DutyTaxCodes.Where(dutyTaxCode => dutyTaxCode.Code == code)
                .FirstOrDefaultAsync();
            var dutyTaxCodeServiceModel = _mapper.Map<DutyTaxCodeServiceModel>(dutyTaxCodeDbModel);
            return dutyTaxCodeServiceModel;
        }
    }
}
