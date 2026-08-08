using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IAdditionalCostService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class AdditionalCostService : IAdditionalCostService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public AdditionalCostService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<AdditionalCostServiceModel> AddAdditionalCostAsync(CreateAdditionalCostServiceModel createAdditionalCostServiceModel)
        {
            try
            {
                var additionalCostDbModel = _mapper.Map<AdditionalCost>(createAdditionalCostServiceModel);
                _apparelProDbContext.AdditionalCosts.Add(additionalCostDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<AdditionalCostServiceModel>(additionalCostDbModel);
            }
            catch (Exception)
            {
                throw new Exception("Additional Cost already exists.");
            }
        }

        public async Task DeleteAdditionalCostAsync(string code)
        {
            var additionalCostDbModel = await _apparelProDbContext.AdditionalCosts
                .Where(additionalCost => additionalCost.Code == code)
                .FirstOrDefaultAsync();
            _apparelProDbContext.AdditionalCosts.Remove(additionalCostDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<bool> DoesAdditionalCostExistAsync(string code)
        {
            return await _apparelProDbContext.AdditionalCosts.AnyAsync(additionalCost => additionalCost.Code == code);
        }

        public async Task<AdditionalCostServiceModel?> GetAdditionalCostByCodeAsync(string code)
        {
            var additionalCostDbModel = await _apparelProDbContext.AdditionalCosts
                .AsNoTracking()
                .Where(additionalCost => additionalCost.Code == code)
                .FirstOrDefaultAsync();
            return _mapper.Map<AdditionalCostServiceModel>(additionalCostDbModel);
        }

        public async Task<PaginationResult<AdditionalCostServiceModel>> GetAdditionalCostsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<AdditionalCost> additionalCostsPagination = _apparelProDbContext.AdditionalCosts.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(AdditionalCost));
                additionalCostsPagination = additionalCostsPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await additionalCostsPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                additionalCostsPagination = additionalCostsPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                additionalCostsPagination = additionalCostsPagination.OrderBy(additionalCost => additionalCost.Code);
            }

            additionalCostsPagination = additionalCostsPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbAdditionalCosts = await additionalCostsPagination.ToListAsync();
            var additionalCostServiceModels = _mapper.Map<IList<AdditionalCostServiceModel>>(filteredDbAdditionalCosts);

            return new PaginationResult<AdditionalCostServiceModel>(pageSize, pageNumber, counter, additionalCostServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateAdditionalCostAsync(UpdateAdditionalCostServiceModel updateAdditionalCostServiceModel)
        {
            var additionalCostDbModel = await _apparelProDbContext.AdditionalCosts
                .Where(additionalCost => additionalCost.Code == updateAdditionalCostServiceModel.Code)
                .FirstOrDefaultAsync();

            additionalCostDbModel!.Description = updateAdditionalCostServiceModel.Description;

            _apparelProDbContext.Update(additionalCostDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
