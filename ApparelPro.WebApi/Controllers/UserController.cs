using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
  
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApparelProUser> _userManager;
        public UserController(IUserService userService, IMapper mapper, UserManager<ApparelProUser> userManager)
        {
            _userService = userService;
            _mapper = mapper;        
            _userManager = userManager;
        }

        [HttpGet("list")]        
        [ProducesResponseType(typeof(IEnumerable<UserAPIModel>),HttpStatusCodes.OK)]
        public async Task<IActionResult> GetUsersAsync()
        {
            var userServiceModels = await _userService.GetUsersAsync();
            var users = _mapper.Map<IEnumerable<UserAPIModel>>(userServiceModels);
            return Ok(users);
        }

        [HttpGet("list/{email}",Name = "GetUserByEmailAsync")]
        [ProducesResponseType(typeof(UserAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetUserByEmailAsync([FromQuery] string email)
        {
            var userServiceModel = await _userService.GetUserByEmailAsync(email);
            if (userServiceModel == null)
            {
                return UnprocessableEntity("User is not available for email :" + email);
            }
            var userAPIModel = _mapper.Map<UserAPIModel>(userServiceModel);
            return Ok(userAPIModel);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisteredUserAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterUserAPIModel registerUserAPIModel)
        {
            if (await UserExists(registerUserAPIModel.Email.ToLower()))
            {
                return BadRequest("Email is taken");
            }

            var registerUserServiceModel = _mapper.Map<RegisterUserServiceModel>(registerUserAPIModel);
            var registeredUserServiceModel = await _userService.RegisterAsync(registerUserServiceModel);
           // var mappedUser = _mapper.Map<RegisteredUserAPIModel>(registeredUserServiceModel);
            return CreatedAtRoute(nameof(GetUserByEmailAsync), new { email = registeredUserServiceModel.Email }, null);         
        }

        private async Task<bool> UserExists(string email)
        {
            var retUser = await _userService.GetUserByEmailAsync(email);
            var user = _mapper.Map<UserAPIModel>(_mapper.Map<UserAPIModel>(retUser));
            return user != null ? true : false;
        }

        [HttpPost]
        [ProducesResponseType(typeof(void), HttpStatusCodes.Created)]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserAPIModel userAPIModel)
        {
            var userServiceModel = _mapper.Map<UserServiceModel>(userAPIModel);
            var createdUserServiceModel = await _userService.AddUserAsync(userServiceModel);
            var createdUserAPIModel = _mapper.Map<UserAPIModel>(createdUserServiceModel);
            return CreatedAtRoute(nameof(GetUserByEmailAsync), new { email = createdUserServiceModel.Email }, null); 
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(RegisterUserAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(UnauthorizedResult), HttpStatusCodes.Unauthorized)]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginUserAPIModel loginUserAPIModel)
        {           
            var user = await _userManager.FindByNameAsync(loginUserAPIModel.Email);
            if (user == null)
            {
                return Unauthorized("Username does not exist");
            }
          
           // var res = await _userManager.IsInRoleAsync(user, "Merchandiser");
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginUserAPIModel.Password))
            {
                return Unauthorized("Invalid password");
               // return Unauthorized(new LoginResult() { Message = "Inavlid password", Success = false });
            }
            
            var logginUserServiceModel = _mapper.Map<LoginUserServiceModel>(loginUserAPIModel);

            var registeredUserServiceModel = await _userService.ValidateLogin(logginUserServiceModel);
            var registeredUserAPIModel = _mapper.Map<RegisteredUserAPIModel>(registeredUserServiceModel);
            return Ok(registeredUserAPIModel);
        }

        [HttpPost("refresh-token")]
     //   [AllowAnonymous]
        [ProducesResponseType(typeof(string), HttpStatusCodes.OK)]        
        public IActionResult RefreshToken([FromBody] object token)
        {            
            var jsonToken = JsonSerializer.Serialize(token);
            var tokenizer = JsonSerializer.Deserialize<Tokenizer>(jsonToken);            
         
            var newToken =  _userService.RefreshTokenUsingExistingToken(tokenizer?.token!);
            var refreshoken = _userService.CreateRefreshToken();
             return Ok(newToken);
        }
       

        [HttpPost("refresh-a-token")]
        [ProducesResponseType(typeof(RegisterUserAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> RefreshToken1(RegisteredUserAPIModel registeredUserAPIModel)
        {
            var registeredUsrServiceModel = _mapper.Map<RegisteredUserServiceModel>(registeredUserAPIModel);
           // var regsiteredUserAPIModel = _mapper.Map<RegisteredUserAPIModel>(await _userService.RefreshTokenUsingExistingToken(registeredUsrServiceModel));
            return Ok();
        }

        //[HttpPost("login")]
        //[ProducesResponseType(typeof(RegisterUserAPIModel), HttpStatusCodes.OK)]
        //[ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        //[ProducesResponseType(typeof(UnauthorizedResult), HttpStatusCodes.Unauthorized)]
        //public async Task<ActionResult> Login(LoginUserAPIModel loginUserAPIModel)
        //{
        //    var user = await _userService.GetUserByEmailAsync(loginUserAPIModel.Email);
        //    if (user == null)
        //    {
        //        return Unauthorized("Username does not exist");
        //    }

        //    var logginUserServiceModel = _mapper.Map<LoginUserServiceModel>(loginUserAPIModel);

        //    var registeredUserServiceModel = await _userService.ValidateLogin(logginUserServiceModel);
        //    if (registeredUserServiceModel == null)
        //    {
        //        return Unauthorized("Invalid password");
        //        //var unauthorizedRegisteredUserServiceModel = new RegisteredUserServiceModel { Email = loginUserAPIModel.Email, Token = null, KnownAs=null, Photo = null };
        //        //var UnauthorizedRegisteredUserAPIModel = _mapper.Map<RegisteredUserAPIModel>(unauthorizedRegisteredUserServiceModel);
        //        // return Unauthorized(UnauthorizedRegisteredUserAPIModel);
        //        // //return Unauthorized(new RegisteredUserServiceModel() { Message = "Inavlid password", Success = false });
        //    }
        //    var registeredUserAPIModel = _mapper.Map<RegisteredUserAPIModel>(registeredUserServiceModel);
        //    return Ok(registeredUserAPIModel);
        //}
    }

   public class Tokenizer
    {
        public string? token { get; set; }
    }
}
