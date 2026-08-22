using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Production.ISectionService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    public class SectionService : ISectionService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public SectionService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<SectionServiceModel>> GetSectionsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Section> query = _apparelProDbContext.Sections.AsNoTracking();

            if (filterColumn != null && filterQuery != null)
            {
                var fr = InputValidator.Validate(filterColumn, filterQuery, typeof(Section));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            var totalCount = await query.CountAsync();

            query = sortColumn != null
                ? query.OrderBy(string.Format("{0} {1}", sortColumn,
                    !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC"))
                : query.OrderBy(s => s.Code);

            var pageResult = await query.Skip(pageSize * pageNumber).Take(pageSize).ToListAsync();
            var serviceModels = _mapper.Map<IList<SectionServiceModel>>(pageResult);

            return new PaginationResult<SectionServiceModel>(
                pageSize, pageNumber, totalCount, serviceModels, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<List<SectionServiceModel>> GetAllSectionsAsync()
        {
            var entities = await _apparelProDbContext.Sections.AsNoTracking().OrderBy(s => s.Code).ToListAsync();
            return _mapper.Map<List<SectionServiceModel>>(entities);
        }

        public async Task<SectionServiceModel> AddSectionAsync(CreateSectionServiceModel createServiceModel)
        {
            var entity = _mapper.Map<Section>(createServiceModel);
            _apparelProDbContext.Sections.Add(entity);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<SectionServiceModel>(entity);
        }

        public async Task UpdateSectionAsync(UpdateSectionServiceModel updateServiceModel)
        {
            var entity = await _apparelProDbContext.Sections
                .FirstOrDefaultAsync(s => s.Code == updateServiceModel.Code)
                ?? throw new KeyNotFoundException($"Section '{updateServiceModel.Code}' was not found.");

            entity.Description = updateServiceModel.Description;
            entity.IsFinal = updateServiceModel.IsFinal;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteSectionAsync(string code)
        {
            var entity = await _apparelProDbContext.Sections.FirstOrDefaultAsync(s => s.Code == code);
            if (entity == null) return;
            _apparelProDbContext.Sections.Remove(entity);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
