using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;



namespace apparelPro.BusinessLogic.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserServiceModel>> GetUsersAsync();
        Task<UserServiceModel> GetUserByIdAsync(int id);     
        Task<UserServiceModel> GetUserByEmailAsync(string username);
        Task<UserServiceModel> AddUserAsync(UserServiceModel user);
        Task UpdateUserAsync(UpdateUserServiceModel user);
        Task DeleteUserAsync(int id);
        Task<RegisterUserServiceModel> RegisterAsync(RegisterUserServiceModel user);
        Task<RegisteredUserServiceModel> ValidateLogin(LoginUserServiceModel loginUserServiceModel);
        string RefreshTokenUsingExistingToken(string token);
        string CreateRefreshToken(); 
     
   //     Task<RegisteredUserServiceModel> RefreshTokenUsingIpAddress(RegisteredUserServiceModel registeredUserServiceModel, string ipAddress);     

        // NEW (2026-08-03) - reads AspNetUsers directly (the real, login-capable user set),
        // NOT the legacy table GetUsersAsync/AddUserAsync above currently use (see design
        // doc section 8). Backs the Users & Groups admin screen's Users panel.
        Task<List<UserWithGroupsServiceModel>> GetIdentityUsersWithGroupsAsync();
        Task AssignUserToGroupAsync(string userId, string groupId);
        Task RemoveUserFromGroupAsync(string userId, string groupId);
    }
}
