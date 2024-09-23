using apparelPro.BusinessLogic.Configuration;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Shared.LookupConstants.ApparelProContext;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Implementation.Registration
{
    public class SecurityService:ISecurityService
    {
        private readonly SymmetricSecurityKey _symmetricSecurityKey;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApparelProUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IOptions<JwtSettings> _jwtSettings;
        public SecurityService(IConfiguration configuration, IMapper mapper, 
            UserManager<ApparelProUser> userManager, IOptions<JwtSettings> jwtSettings) {
            
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (userManager == null)
            {
                throw new ArgumentNullException(nameof(userManager));
            }
            if (jwtSettings == null)
            {
                throw new ArgumentNullException(nameof(jwtSettings));
            }
            _configuration = configuration;
            _userManager = userManager;
            _mapper = mapper;
            _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:TokenKey"]!));
            _jwtSettings = jwtSettings;
        }       

        public async Task<string> CreateTokenAsync(RegisteredUserServiceModel userServiceModel)
        {           
            var credentials = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);

            var user = _mapper.Map<ApparelProUser>(userServiceModel);
            
            var JwtSettingsConfig = _jwtSettings.Value;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = JwtSettingsConfig.Issuer,                
                Audience = JwtSettingsConfig.Audience,                                
                Subject = new ClaimsIdentity(await GetClaimsAsync(user)),
                SigningCredentials = credentials,                
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(JwtSettingsConfig.WillExpireInMinutes)),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string CreateTokenAsync(IEnumerable<Claim> claims)
        {
            var credentials = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);

            var JwtSettingsConfig = _jwtSettings.Value;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = JwtSettingsConfig.Issuer,                
                Audience = JwtSettingsConfig.Audience,                
                // Claims = GetClaimsAsync(user),
                //Subject = new ClaimsIdentity(claims),
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,                
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(JwtSettingsConfig.WillExpireInMinutes)),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // https://www.c-sharpcorner.com/article/how-to-handle-token-expiry-in-angular-with-net-core/

        private async Task<IEnumerable<Claim>> GetClaimsAsync(ApparelProUser user)
        {
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, user.Email) };
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }      

        public string CreateRefreshToken()
        {
            var bytes = new byte[32];
            using var randomNumber = RandomNumberGenerator.Create();
            randomNumber.GetBytes(bytes);
            var refreshToken = Convert.ToBase64String(bytes);
            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalUsingExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _symmetricSecurityKey,
                ValidateLifetime = false,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principle = tokenHandler.ValidateToken(token!, tokenValidationParameters, out SecurityToken retrievedSecurityToken);
            if (retrievedSecurityToken is not JwtSecurityToken 
                jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, 
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principle ?? throw new SecurityTokenException();
        }
    }
}
