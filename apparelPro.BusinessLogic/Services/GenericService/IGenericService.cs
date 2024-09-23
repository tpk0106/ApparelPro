namespace apparelPro.BusinessLogic.Services.GenericService
{
    public interface IGenericService<C> where C:class 
    {
        Task<C> GetUnitByCodeAsync(string code);
    }
}
