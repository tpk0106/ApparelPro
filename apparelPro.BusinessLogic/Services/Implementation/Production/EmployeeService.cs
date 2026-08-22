using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IEmployeeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public EmployeeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<EmployeeServiceModel>> GetEmployeesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Employee> query = _apparelProDbContext.Employees.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(Employee));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<EmployeeServiceModel>>(pageResult);

            return new PaginationResult<EmployeeServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<EmployeeServiceModel?> GetEmployeeByEmployeeCodeAsync(string employeeCode)
        {
            var entity = await _apparelProDbContext.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
            return entity == null ? null : _mapper.Map<EmployeeServiceModel>(entity);
        }

        public async Task<EmployeeServiceModel> AddEmployeeAsync(CreateEmployeeServiceModel createServiceModel)
        {
            var entity = _mapper.Map<Employee>(createServiceModel);
            _apparelProDbContext.Employees.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<EmployeeServiceModel>(entity);
        }

        public async Task UpdateEmployeeAsync(UpdateEmployeeServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == updateServiceModel.EmployeeCode)
                ?? throw new KeyNotFoundException($"Employee '{updateServiceModel.EmployeeCode}' was not found.");

            entity.Name = updateServiceModel.Name;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(string employeeCode)
        {
            var entity = await _apparelProDbContext.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
            if (entity == null) return;
            _apparelProDbContext.Employees.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
