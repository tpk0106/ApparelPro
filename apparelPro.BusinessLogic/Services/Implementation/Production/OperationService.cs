using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IOperationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class OperationService : IOperationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public OperationService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<OperationServiceModel>> GetOperationsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Operation> query = _apparelProDbContext.Operations.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(Operation));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<OperationServiceModel>>(pageResult);

            return new PaginationResult<OperationServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<OperationServiceModel?> GetOperationByOperationCodeAsync(string operationCode)
        {
            var entity = await _apparelProDbContext.Operations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OperationCode == operationCode);
            return entity == null ? null : _mapper.Map<OperationServiceModel>(entity);
        }

        public async Task<OperationServiceModel> AddOperationAsync(CreateOperationServiceModel createServiceModel)
        {
            var entity = _mapper.Map<Operation>(createServiceModel);
            _apparelProDbContext.Operations.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<OperationServiceModel>(entity);
        }

        public async Task UpdateOperationAsync(UpdateOperationServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.Operations
                .FirstOrDefaultAsync(o => o.OperationCode == updateServiceModel.OperationCode)
                ?? throw new KeyNotFoundException($"Operation '{updateServiceModel.OperationCode}' was not found.");

            entity.Description = updateServiceModel.Description;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteOperationAsync(string operationCode)
        {
            var entity = await _apparelProDbContext.Operations.FirstOrDefaultAsync(o => o.OperationCode == operationCode);
            if (entity == null) return;
            _apparelProDbContext.Operations.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
