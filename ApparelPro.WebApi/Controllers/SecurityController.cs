using apparelPro.BusinessLogic.Services;
using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/security")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityService _securityService;
        private readonly UserManager<ApparelProUser> _userManager;
        private readonly UserIdentityDbContext _userIdentityDbContext;
        private readonly IMapper _mapper;
        public SecurityController(ISecurityService securityService, UserManager<ApparelProUser> userManager,
            UserIdentityDbContext userIdentityDbContext, IMapper mapper)
        {
            _securityService = securityService;
            _userManager = userManager;
            _userIdentityDbContext = userIdentityDbContext;
            _mapper = mapper;
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(TokenAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.OK)]        
        public async Task<IActionResult> RefreshToken([FromBody] TokenAPIModel tokenAPIModel)
        {
            if (tokenAPIModel == null)
            {
                return BadRequest("Invalid token and invalid refresh token");
            }

            ClaimsPrincipal principal = _securityService.GetPrincipalUsingExpiredToken(tokenAPIModel.Token);
            if (principal == null)
            {
                return BadRequest("Invalid token or current token does not belong to the avaialble identity claims");
            }            

            var email = principal.Identity!.Name;
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, email!) };

            var existingRefreshToken = tokenAPIModel.RefreshToken;

            var user = _userIdentityDbContext.Users.Where(u => u.Email == email).FirstOrDefault();            
            
            if (user == null ||   user.RefreshToken != existingRefreshToken || user.RefreshTokenExpiry <= DateTime.Now)
            {
                return BadRequest("Refresh token expired....Please login");
            }

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var newtoken = _securityService.CreateTokenAsync(claims);
            var newRefreshToken = _securityService.CreateRefreshToken();

            tokenAPIModel.Token = newtoken;
            tokenAPIModel.RefreshToken = newRefreshToken;

            user.RefreshToken = newRefreshToken;

            await _userIdentityDbContext.SaveChangesAsync();
            return Ok(tokenAPIModel);
        }

        [HttpPost("revoke")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(NoContentResult),HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(BadRequestResult),HttpStatusCodes.BadRequest)]
        public IActionResult Revoke(string? user)
        {
            var apparelProUser  =_userIdentityDbContext.Users.Where(u => u.UserName == user).FirstOrDefault();
            if(apparelProUser == null)
            {
                return BadRequest("Invalid user");
            }
            apparelProUser.RefreshToken = null;
            return NoContent();
        }
    }
}
