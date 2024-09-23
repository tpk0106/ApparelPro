using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserServiceModel>> GetUsersAsync();
        Task<UserServiceModel> GetUserByIdAsync(int id);     
        Task<UserServiceModel> GetUserByEmailAsync(string username);
        Task<UserServiceModel> AddUserAsync(UserServiceModel user);
        Task UpdateUser(UserServiceModel user);
        Task DeleteUserAsync(int id);
        Task<RegisterUserServiceModel> RegisterAsync(RegisterUserServiceModel user);
        Task<RegisteredUserServiceModel> ValidateLogin(LoginUserServiceModel loginUserServiceModel);
        string RefreshTokenUsingExistingToken(string token);
        string CreateRefreshToken(); 
     
   //     Task<RegisteredUserServiceModel> RefreshTokenUsingIpAddress(RegisteredUserServiceModel registeredUserServiceModel, string ipAddress);     
    }
}
