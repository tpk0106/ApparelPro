using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class BasisService : IBasisService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        public BasisService(IMapper mapper,  ApparelProDbContext apparelProDbContext, ILookupConstants lookupConstants)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;                
        }

        public async Task<BasisServiceModel> AddBasisAsync(CreateBasisServiceModel createBasisServiceModel)
        {
            try
            {
                var basisDbModel = _mapper.Map<Basis>(createBasisServiceModel);
                _apparelProDbContext.Basis.Add(basisDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<BasisServiceModel>(basisDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Basis already exists");
            }            
        }

        public async Task DeleteBasisAsync(string code)
        {
            try
            {
                var basisDbModel = await _apparelProDbContext.Basis
                    .Where(basis => basis.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.Basis.Remove(basisDbModel!);
                await _apparelProDbContext.SaveChangesAsync();                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }
     
        public async Task<PaginationResult<BasisServiceModel>> GetBasisesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Basis> basisPagination = _apparelProDbContext.Basis.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Basis));
                basisPagination = basisPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await basisPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                basisPagination = basisPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
                //sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                //basisPagination = sortOrder.ToUpper() == "ASC" ?
                //    basisPagination.OrderByColumn(sortColumn.Trim()) :
                //    basisPagination.OrderByColumnDescending(sortColumn.Trim());
            }
            basisPagination = basisPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCountries = await basisPagination.ToListAsync();
            var basisServiceModels = _mapper.Map<IList<BasisServiceModel>>(filteredDbCountries);

            return new PaginationResult<BasisServiceModel>(pageSize, pageNumber, counter, basisServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateBasisAsync(UpdateBasisServiceModel updateBasisServiceModel)
        {
            try
            {
                var basisDbModel = await _apparelProDbContext.Basis
                    .Where(basis => basis.Code == updateBasisServiceModel.Code)
                    .FirstOrDefaultAsync();
                if(basisDbModel != null)
                {
                    basisDbModel?.Description = updateBasisServiceModel.Description;
                    basisDbModel?.ValueAdd = updateBasisServiceModel.ValueAdd;
                    _apparelProDbContext.Basis.Update(basisDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<BasisServiceModel> GetBasisByCodeAsync(string code)
        {
            var basisDbModel = await _apparelProDbContext.Basis.Where(basis => basis.Code == code)
                .FirstOrDefaultAsync();
            var basisServiceModel = _mapper.Map<BasisServiceModel>(basisDbModel);
            return basisServiceModel;
        }
    }
}
