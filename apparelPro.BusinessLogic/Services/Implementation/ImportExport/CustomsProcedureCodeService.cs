using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsProcedureCodeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class CustomsProcedureCodeService : ICustomsProcedureCodeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public CustomsProcedureCodeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<CustomsProcedureCodeServiceModel> AddCustomsProcedureCodeAsync(CreateCustomsProcedureCodeServiceModel createCustomsProcedureCodeServiceModel)
        {
            try
            {
                var customsProcedureCodeDbModel = _mapper.Map<CustomsProcedureCode>(createCustomsProcedureCodeServiceModel);
                _apparelProDbContext.CustomsProcedureCodes.Add(customsProcedureCodeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<CustomsProcedureCodeServiceModel>(customsProcedureCodeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Customs Procedure Code already exists");
            }
        }

        public async Task DeleteCustomsProcedureCodeAsync(string code)
        {
            try
            {
                var customsProcedureCodeDbModel = await _apparelProDbContext.CustomsProcedureCodes
                    .Where(customsProcedureCode => customsProcedureCode.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.CustomsProcedureCodes.Remove(customsProcedureCodeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<CustomsProcedureCodeServiceModel>> GetCustomsProcedureCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<CustomsProcedureCode> customsProcedureCodePagination = _apparelProDbContext.CustomsProcedureCodes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(CustomsProcedureCode));
                customsProcedureCodePagination = customsProcedureCodePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await customsProcedureCodePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                customsProcedureCodePagination = customsProcedureCodePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            customsProcedureCodePagination = customsProcedureCodePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCustomsProcedureCodes = await customsProcedureCodePagination.ToListAsync();
            var customsProcedureCodeServiceModels = _mapper.Map<IList<CustomsProcedureCodeServiceModel>>(filteredDbCustomsProcedureCodes);

            return new PaginationResult<CustomsProcedureCodeServiceModel>(pageSize, pageNumber, counter, customsProcedureCodeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateCustomsProcedureCodeAsync(UpdateCustomsProcedureCodeServiceModel updateCustomsProcedureCodeServiceModel)
        {
            try
            {
                var customsProcedureCodeDbModel = await _apparelProDbContext.CustomsProcedureCodes
                    .Where(customsProcedureCode => customsProcedureCode.Code == updateCustomsProcedureCodeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (customsProcedureCodeDbModel != null)
                {
                    customsProcedureCodeDbModel.Description = updateCustomsProcedureCodeServiceModel.Description;
                    _apparelProDbContext.CustomsProcedureCodes.Update(customsProcedureCodeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<CustomsProcedureCodeServiceModel> GetCustomsProcedureCodeByCodeAsync(string code)
        {
            var customsProcedureCodeDbModel = await _apparelProDbContext.CustomsProcedureCodes.Where(customsProcedureCode => customsProcedureCode.Code == code)
                .FirstOrDefaultAsync();
            var customsProcedureCodeServiceModel = _mapper.Map<CustomsProcedureCodeServiceModel>(customsProcedureCodeDbModel);
            return customsProcedureCodeServiceModel;
        }
    }
}
