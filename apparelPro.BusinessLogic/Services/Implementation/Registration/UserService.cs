using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace apparelPro.BusinessLogic.Services.Implementation.Registration
{
    public class UserService : IUserService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IMapper _mapper;
        private readonly ILookupConstants _lookupConstants;
        private readonly ISecurityService _securityService;
        private readonly UserIdentityDbContext _userIdentityDbContext;
        private readonly IConfiguration _configuration;
        public UserService(ApparelProDbContext apparelProDbContext, IMapper mapper, ILookupConstants lookupConstants, 
            ISecurityService securityService, UserIdentityDbContext userIdentityDbContext, IConfiguration configuration)
        {
            if (apparelProDbContext == null)
            {
                throw new ArgumentNullException(nameof(apparelProDbContext));
            }
            if (userIdentityDbContext == null)
            {
                throw new ArgumentNullException(nameof(userIdentityDbContext));
            }
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (lookupConstants == null)
            {
                throw new ArgumentNullException(nameof(lookupConstants));
            }
            if (securityService == null)
            {
                throw new ArgumentNullException(nameof(securityService));
            }
            if(configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
            _lookupConstants = lookupConstants;
            _securityService = securityService;
            _userIdentityDbContext = userIdentityDbContext;
            _configuration = configuration;
        }

        public async Task<UserServiceModel> AddUserAsync(UserServiceModel userServiceModel)
        {
            var userDbModel = _mapper.Map<User>(userServiceModel);
            _apparelProDbContext.Users.Add(userDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            var createdUserServiceModel = _mapper.Map<UserServiceModel>(userDbModel);
            return createdUserServiceModel;
        }      

        public async Task<UserServiceModel> GetUserByEmailAsync(string email)
        {
            var userDbModel = await _apparelProDbContext.Users.Where(user => user.Email == email).FirstOrDefaultAsync();
            var userServiceModel = _mapper.Map<UserServiceModel>(userDbModel);
            return userServiceModel;
        }
        public async Task<IEnumerable<UserServiceModel>> GetUsersAsync()
        {
            var userDbModels = await _apparelProDbContext.Users.ToListAsync();
            var userServiceModels = _mapper.Map<IEnumerable<UserServiceModel>>(userDbModels);
            return userServiceModels;
        }

        public async Task<RegisterUserServiceModel> RegisterAsync(RegisterUserServiceModel registerUserServiceModel)
        {
            var user = _mapper.Map<User>(registerUserServiceModel);
            using var hmac = new HMACSHA512();

            user.Email = registerUserServiceModel.Email.ToLower();            
            user.PasswordSalt = hmac.Key;

            _apparelProDbContext.Users.Add(user);
            await _apparelProDbContext.SaveChangesAsync();

            var registeredUser = _mapper.Map<RegisterUserServiceModel>(user);            
            return registeredUser;
        }     

        public async Task<RegisteredUserServiceModel> ValidateLogin(LoginUserServiceModel loginUserServiceModel)
        {
            var user = await _userIdentityDbContext.Users
                .Where(user => user.Email == loginUserServiceModel.Email)               
                .FirstOrDefaultAsync();

            var registeredUserServiceModel = _mapper.Map<RegisteredUserServiceModel>(user);
            registeredUserServiceModel.Token = await CreateToken(registeredUserServiceModel);
            registeredUserServiceModel.RefreshToken = CreateRefreshToken();
            var duration = Convert.ToInt32(_configuration.GetRequiredSection("RefreshTokenExpiry").Value);
            registeredUserServiceModel.RefreshTokenExpiry = DateTime.Now.AddDays(duration);

            user!.RefreshToken = registeredUserServiceModel.RefreshToken;
            user.RefreshTokenExpiry = registeredUserServiceModel.RefreshTokenExpiry;

            await _userIdentityDbContext.SaveChangesAsync();

            return registeredUserServiceModel;
        }
        
        public string RefreshTokenUsingExistingToken(string token)
        {           
            var refreshToken =  _securityService.CreateRefreshToken();            
            return refreshToken;         
        }      

        private async Task<string> CreateToken(RegisteredUserServiceModel registeredUserServiceModel)
        {
            return await _securityService.CreateTokenAsync(registeredUserServiceModel);
        }
        public Task UpdateUser(UserServiceModel user)
        {
            throw new NotImplementedException();
        }
        public string CreateRefreshToken()
        {
            var refreshToken = _securityService.CreateRefreshToken();
            return refreshToken;
        }
        public Task DeleteUserAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<UserServiceModel> GetUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
