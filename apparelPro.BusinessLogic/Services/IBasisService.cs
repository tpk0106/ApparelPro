using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IBasisService
    {
        Task<PaginationResult<BasisServiceModel>> GetBasisesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

        Task GetBasisAsync();
        Task GetBasisByCodeAsync(string code);
        Task AddBasisAsync(CreateBasisServiceModel createBasisServiceModel);
        Task UpdateBasisAsync(UpdateBasisServiceModel  updateBasisServiceModel);
        Task DeleteBasissAsync(string code);
    }

    //public interface IBasisService<T> where T : class
    //{
    //    Task<PaginationResult<CountryServiceModel>> GetBasisAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
    //    Task<IEnumerable<T>> GetBasisAsync();
    //    Task<T> GetBasisByCodeAsync(string code);
    //    Task<T> AddBasisAsync(CreateBasisServiceModel createBasisServiceModel);
    //    Task UpdateBasisAsync(UpdateBasisServiceModel updateBasisServiceModel);
    //    Task DeleteBasissAsync(string code);
    //}
}
