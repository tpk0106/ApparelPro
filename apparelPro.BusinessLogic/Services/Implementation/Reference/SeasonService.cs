using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.ISeasonService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class SeasonService : ISeasonService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public SeasonService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<SeasonServiceModel>> GetSeasonsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Season> seasonQuery = _apparelProDbContext.Seasons.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Season));
                seasonQuery = seasonQuery.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await seasonQuery.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                seasonQuery = seasonQuery.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            seasonQuery = seasonQuery
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredSeasons = await seasonQuery.ToListAsync();
            var seasonServiceModels = _mapper.Map<IList<SeasonServiceModel>>(filteredSeasons);

            return new PaginationResult<SeasonServiceModel>(pageSize, pageNumber, counter, seasonServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<SeasonServiceModel?> GetSeasonByCodeAsync(string code)
        {
            var seasonDbModel = await _apparelProDbContext.Seasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Code == code);
            return seasonDbModel == null ? null : _mapper.Map<SeasonServiceModel>(seasonDbModel);
        }

        public async Task<SeasonServiceModel> AddSeasonAsync(CreateSeasonServiceModel createSeasonServiceModel)
        {
            try
            {
                var seasonDbModel = _mapper.Map<Season>(createSeasonServiceModel);
                _apparelProDbContext.Seasons.Add(seasonDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<SeasonServiceModel>(seasonDbModel);
            }
            catch (Exception)
            {
                throw new Exception("Season already exists");
            }
        }

        public async Task UpdateSeasonAsync(UpdateSeasonServiceModel updateSeasonServiceModel)
        {
            try
            {
                var seasonDbModel = await _apparelProDbContext.Seasons
                    .FirstOrDefaultAsync(s => s.Code == updateSeasonServiceModel.Code);
                if (seasonDbModel != null)
                {
                    seasonDbModel.Description = updateSeasonServiceModel.Description;
                    _apparelProDbContext.Seasons.Update(seasonDbModel);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteSeasonAsync(string code)
        {
            try
            {
                var seasonDbModel = await _apparelProDbContext.Seasons
                    .FirstOrDefaultAsync(s => s.Code == code);
                _apparelProDbContext.Seasons.Remove(seasonDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Mirrors the DB's own protection: a Season still referenced by a
                // PurchaseOrder (FK_PurchaseOrders_Seasons_Season) can't be deleted.
                throw new Exception(ex.Message);
            }
        }
    }
}
