using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IClearanceOfficeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class ClearanceOfficeService : IClearanceOfficeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public ClearanceOfficeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ClearanceOfficeServiceModel> AddClearanceOfficeAsync(CreateClearanceOfficeServiceModel createClearanceOfficeServiceModel)
        {
            try
            {
                var clearanceOfficeDbModel = _mapper.Map<ClearanceOffice>(createClearanceOfficeServiceModel);
                _apparelProDbContext.ClearanceOffices.Add(clearanceOfficeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<ClearanceOfficeServiceModel>(clearanceOfficeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Clearance Office already exists");
            }
        }

        public async Task DeleteClearanceOfficeAsync(string code)
        {
            try
            {
                var clearanceOfficeDbModel = await _apparelProDbContext.ClearanceOffices
                    .Where(clearanceOffice => clearanceOffice.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.ClearanceOffices.Remove(clearanceOfficeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<ClearanceOfficeServiceModel>> GetClearanceOfficesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ClearanceOffice> clearanceOfficePagination = _apparelProDbContext.ClearanceOffices.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(ClearanceOffice));
                clearanceOfficePagination = clearanceOfficePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await clearanceOfficePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                clearanceOfficePagination = clearanceOfficePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            clearanceOfficePagination = clearanceOfficePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbClearanceOffices = await clearanceOfficePagination.ToListAsync();
            var clearanceOfficeServiceModels = _mapper.Map<IList<ClearanceOfficeServiceModel>>(filteredDbClearanceOffices);

            return new PaginationResult<ClearanceOfficeServiceModel>(pageSize, pageNumber, counter, clearanceOfficeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateClearanceOfficeAsync(UpdateClearanceOfficeServiceModel updateClearanceOfficeServiceModel)
        {
            try
            {
                var clearanceOfficeDbModel = await _apparelProDbContext.ClearanceOffices
                    .Where(clearanceOffice => clearanceOffice.Code == updateClearanceOfficeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (clearanceOfficeDbModel != null)
                {
                    clearanceOfficeDbModel.Description = updateClearanceOfficeServiceModel.Description;
                    _apparelProDbContext.ClearanceOffices.Update(clearanceOfficeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<ClearanceOfficeServiceModel> GetClearanceOfficeByCodeAsync(string code)
        {
            var clearanceOfficeDbModel = await _apparelProDbContext.ClearanceOffices.Where(clearanceOffice => clearanceOffice.Code == code)
                .FirstOrDefaultAsync();
            var clearanceOfficeServiceModel = _mapper.Map<ClearanceOfficeServiceModel>(clearanceOfficeDbModel);
            return clearanceOfficeServiceModel;
        }
    }
}
