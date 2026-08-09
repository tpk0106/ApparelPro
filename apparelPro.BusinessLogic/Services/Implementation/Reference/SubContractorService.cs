using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.ISubContractorService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    // Same simple CRUD template as AdditionalCostService - see SubContractor.cs for why
    // this table exists and how narrowly scoped it is (Code + Name only).
    public class SubContractorService : ISubContractorService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public SubContractorService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
        }

        public async Task<SubContractorServiceModel> AddSubContractorAsync(CreateSubContractorServiceModel createSubContractorServiceModel)
        {
            try
            {
                var subContractorDbModel = _mapper.Map<SubContractor>(createSubContractorServiceModel);
                _apparelProDbContext.SubContractors.Add(subContractorDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<SubContractorServiceModel>(subContractorDbModel);
            }
            catch (Exception)
            {
                throw new Exception("Sub Contractor already exists.");
            }
        }

        public async Task DeleteSubContractorAsync(string code)
        {
            var subContractorDbModel = await _apparelProDbContext.SubContractors
                .Where(subContractor => subContractor.Code == code)
                .FirstOrDefaultAsync();
            _apparelProDbContext.SubContractors.Remove(subContractorDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<bool> DoesSubContractorExistAsync(string code)
        {
            return await _apparelProDbContext.SubContractors.AnyAsync(subContractor => subContractor.Code == code);
        }

        public async Task<SubContractorServiceModel?> GetSubContractorByCodeAsync(string code)
        {
            var subContractorDbModel = await _apparelProDbContext.SubContractors
                .AsNoTracking()
                .Where(subContractor => subContractor.Code == code)
                .FirstOrDefaultAsync();
            return _mapper.Map<SubContractorServiceModel>(subContractorDbModel);
        }

        public async Task<PaginationResult<SubContractorServiceModel>> GetSubContractorsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<SubContractor> subContractorsPagination = _apparelProDbContext.SubContractors.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(SubContractor));
                subContractorsPagination = subContractorsPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await subContractorsPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                subContractorsPagination = subContractorsPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                subContractorsPagination = subContractorsPagination.OrderBy(subContractor => subContractor.Code);
            }

            subContractorsPagination = subContractorsPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbSubContractors = await subContractorsPagination.ToListAsync();
            var subContractorServiceModels = _mapper.Map<IList<SubContractorServiceModel>>(filteredDbSubContractors);

            return new PaginationResult<SubContractorServiceModel>(pageSize, pageNumber, counter, subContractorServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateSubContractorAsync(UpdateSubContractorServiceModel updateSubContractorServiceModel)
        {
            var subContractorDbModel = await _apparelProDbContext.SubContractors
                .Where(subContractor => subContractor.Code == updateSubContractorServiceModel.Code)
                .FirstOrDefaultAsync();

            subContractorDbModel!.Name = updateSubContractorServiceModel.Name;

            _apparelProDbContext.Update(subContractorDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
