using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;

using ApparelPro.Data.Models.References;
using ApparelPro.Data;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using apparelPro.BusinessLogic.Misc;
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

        public Task AddBasisAsync(CreateBasisServiceModel createBasisServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBasissAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task GetBasisAsync()
        {
            throw new NotImplementedException();
        }

        public Task GetBasisByCodeAsync(string code)
        {
            throw new NotImplementedException();
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

        public Task UpdateBasisAsync(UpdateBasisServiceModel updateBasisServiceModel)
        {
            throw new NotImplementedException();
        }
    }
}
