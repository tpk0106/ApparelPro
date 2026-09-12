using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ITaxBaseCodeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class TaxBaseCodeService : ITaxBaseCodeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public TaxBaseCodeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<TaxBaseCodeServiceModel> AddTaxBaseCodeAsync(CreateTaxBaseCodeServiceModel createTaxBaseCodeServiceModel)
        {
            try
            {
                var taxBaseCodeDbModel = _mapper.Map<TaxBaseCode>(createTaxBaseCodeServiceModel);
                _apparelProDbContext.TaxBaseCodes.Add(taxBaseCodeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<TaxBaseCodeServiceModel>(taxBaseCodeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Tax Base Code already exists");
            }
        }

        public async Task DeleteTaxBaseCodeAsync(string code)
        {
            try
            {
                var taxBaseCodeDbModel = await _apparelProDbContext.TaxBaseCodes
                    .Where(taxBaseCode => taxBaseCode.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.TaxBaseCodes.Remove(taxBaseCodeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<TaxBaseCodeServiceModel>> GetTaxBaseCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<TaxBaseCode> taxBaseCodePagination = _apparelProDbContext.TaxBaseCodes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(TaxBaseCode));
                taxBaseCodePagination = taxBaseCodePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await taxBaseCodePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                taxBaseCodePagination = taxBaseCodePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            taxBaseCodePagination = taxBaseCodePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbTaxBaseCodes = await taxBaseCodePagination.ToListAsync();
            var taxBaseCodeServiceModels = _mapper.Map<IList<TaxBaseCodeServiceModel>>(filteredDbTaxBaseCodes);

            return new PaginationResult<TaxBaseCodeServiceModel>(pageSize, pageNumber, counter, taxBaseCodeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateTaxBaseCodeAsync(UpdateTaxBaseCodeServiceModel updateTaxBaseCodeServiceModel)
        {
            try
            {
                var taxBaseCodeDbModel = await _apparelProDbContext.TaxBaseCodes
                    .Where(taxBaseCode => taxBaseCode.Code == updateTaxBaseCodeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (taxBaseCodeDbModel != null)
                {
                    taxBaseCodeDbModel.Description = updateTaxBaseCodeServiceModel.Description;
                    _apparelProDbContext.TaxBaseCodes.Update(taxBaseCodeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<TaxBaseCodeServiceModel> GetTaxBaseCodeByCodeAsync(string code)
        {
            var taxBaseCodeDbModel = await _apparelProDbContext.TaxBaseCodes.Where(taxBaseCode => taxBaseCode.Code == code)
                .FirstOrDefaultAsync();
            var taxBaseCodeServiceModel = _mapper.Map<TaxBaseCodeServiceModel>(taxBaseCodeDbModel);
            return taxBaseCodeServiceModel;
        }
    }
}
