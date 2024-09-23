using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using System.Security.Claims;

namespace apparelPro.BusinessLogic.Services
{
    public interface ISecurityService
    {
        Task<string> CreateTokenAsync(RegisteredUserServiceModel userServiceModel);
        string CreateTokenAsync(IEnumerable<Claim> claims);
        string CreateRefreshToken();   
        ClaimsPrincipal GetPrincipalUsingExpiredToken(string token);
    }
}
