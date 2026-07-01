using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using apparelPro.BusinessLogic.Services.Models.Reference.IDepartmentService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public DepartmentService(IMapper mapper, ApparelProDbContext apparelProReferenceDbContext,
            ILookupConstants lookupConstants, IDistributedCache distributedCache)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProReferenceDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }
        public async Task<PaginationResult<DepartmentServiceModel>> GetDepartmentsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Department> DepartmentPagination = _apparelProDbContext.Departments.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Department));
                DepartmentPagination = DepartmentPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await DepartmentPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                DepartmentPagination = DepartmentPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            List<Department>? result = null;

            DepartmentPagination = DepartmentPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            result = await DepartmentPagination.ToListAsync();

            var filteredDbCountries = result;
            var departmentServiceModels = _mapper.Map<IList<DepartmentServiceModel>>(filteredDbCountries);

            return new PaginationResult<DepartmentServiceModel>(pageSize, pageNumber, counter, departmentServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<IEnumerable<DepartmentServiceModel>> GetDepartmentsLookup()
        {
            var departments = await _apparelProDbContext.Departments
                   .AsNoTracking()
                   .OrderBy(d => d.DepartmentCode)
                   .ToListAsync();

            var departmentServiceModels = _mapper.Map<IEnumerable<DepartmentServiceModel>>((IEnumerable<Department>) departments);

            return departmentServiceModels;
        }
    }
}
