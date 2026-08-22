using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.IComponentOperationTemplateService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class ComponentOperationTemplateService : IComponentOperationTemplateService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public ComponentOperationTemplateService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<ComponentOperationTemplateServiceModel>> GetComponentOperationTemplatesAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ComponentOperationTemplate> query = _apparelProDbContext.ComponentOperationTemplates.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(ComponentOperationTemplate));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                query = query.OrderBy(t => t.ComponentCode).ThenBy(t => t.OperationSequence);
            }

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<ComponentOperationTemplateServiceModel>>(pageResult);

            return new PaginationResult<ComponentOperationTemplateServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<List<ComponentOperationTemplateServiceModel>> GetTemplatesByComponentAsync(string componentCode)
        {
            var entities = await _apparelProDbContext.ComponentOperationTemplates
                .AsNoTracking()
                .Where(t => t.ComponentCode == componentCode)
                .OrderBy(t => t.OperationSequence)
                .ToListAsync();

            return _mapper.Map<List<ComponentOperationTemplateServiceModel>>(entities);
        }

        public async Task<ComponentOperationTemplateServiceModel> AddComponentOperationTemplateAsync(CreateComponentOperationTemplateServiceModel createServiceModel)
        {
            var entity = _mapper.Map<ComponentOperationTemplate>(createServiceModel);
            _apparelProDbContext.ComponentOperationTemplates.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<ComponentOperationTemplateServiceModel>(entity);
        }

        public async Task UpdateComponentOperationTemplateAsync(UpdateComponentOperationTemplateServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.ComponentOperationTemplates
                .FirstOrDefaultAsync(t => t.ComponentCode == updateServiceModel.ComponentCode
                                        && t.OperationSequence == updateServiceModel.OperationSequence)
                ?? throw new KeyNotFoundException(
                    $"Component operation template '{updateServiceModel.ComponentCode}/{updateServiceModel.OperationSequence}' was not found.");

            entity.OperationCode = updateServiceModel.OperationCode;
            entity.MachineTypeCode = updateServiceModel.MachineTypeCode;
            entity.Sam = updateServiceModel.Sam;
            entity.NumberOfMachines = updateServiceModel.NumberOfMachines;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteComponentOperationTemplateAsync(string componentCode, int operationSequence)
        {
            var entity = await _apparelProDbContext.ComponentOperationTemplates
                .FirstOrDefaultAsync(t => t.ComponentCode == componentCode && t.OperationSequence == operationSequence);
            if (entity == null) return;
            _apparelProDbContext.ComponentOperationTemplates.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
