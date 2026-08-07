using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.Authorization;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
//using UpdateUSMAlias = apparelPro.BusinessLogic.Services.Models.Registration.IUserService;


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


        /// <summary>
        /// Registers a new user 
        /// </summary>
        /// <param name="UserAPIModel">A DTO containing a user data</param>
        /// <returns>A 200 - list of DTO with success.</returns>
        /// <response code="200">List of Users</response>
        /// /// <response code="400">Invalid data</response>
        /// <response code="500">An error occurred</response>
        [HttpGet("list")]
        // SECURITY FIX (2026-08-03): was [AllowAnonymous] - any unauthenticated caller could
        // list every user in the system. Locked to Administrator, consistent with every other
        // admin-only surface in this app (see AccessPolicies.AdministratorOnly).
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
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
        public async Task<IActionResult> GetUserByEmailAsync([FromRoute] string email)
        {
            var userServiceModel = await _userService.GetUserByEmailAsync(email);
            if (userServiceModel == null)
            {
                return UnprocessableEntity("User is not available for email :" + email);
            }
            var userAPIModel = _mapper.Map<UserAPIModel>(userServiceModel);
            return Ok(userAPIModel);
        }

        //[HttpGet("list/{knownAs}", Name = "GetUserByKnownASAsync")]
        //[ProducesResponseType(typeof(UserAPIModel), HttpStatusCodes.OK)]
        //[ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        //public async Task<IActionResult> GetUserByKnownAsAsync([FromQuery] string knownAs)
        //{
        //    var userServiceModel = await _userService.GetUserByEmailAsync(knownAs);
        //    if (userServiceModel == null)
        //    {
        //        return UnprocessableEntity("User is not available for email :" + knownAs);
        //    }
        //    var userAPIModel = _mapper.Map<UserAPIModel>(userServiceModel);
        //    return Ok(userAPIModel);
        //}

        [HttpPost("register")]
        // SECURITY FIX (2026-08-03): was [AllowAnonymous] - any unauthenticated caller could
        // self-register an account. Per explicit decision (see the "Users, Groups & Permissions"
        // design doc, section 7): this is an internal ERP, not a public sign-up product - account
        // creation is Administrator-only from here on. The frontend's public Sign-Up page will
        // need its own follow-up removal/redirect (queued, not yet done) - this endpoint being
        // gated is what actually closes the hole.
        [ProducesResponseType(typeof(RegisteredUserAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        public async Task<ActionResult> Register(RegisterUserAPIModel registerUserAPIModel)
        {
            if (await UserExists(registerUserAPIModel.Email.ToLower()))
            {                
                return BadRequest(new { message = "Email is already registered" });
            }

            try
            {
                var registerUserServiceModel = _mapper.Map<RegisterUserServiceModel>(registerUserAPIModel);
                var registeredUserServiceModel = await _userService.RegisterAsync(registerUserServiceModel);

                // var mappedUser = _mapper.Map<RegisteredUserAPIModel>(registeredUserServiceModel);
                return CreatedAtRoute(nameof(GetUserByEmailAsync), new { email = registeredUserServiceModel.Email }, null);
            }
            catch (Exception ex)
            {
                // 2. Catch service exceptions and format them safely into a JSON object package
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<bool> UserExists(string email)
        {
            // Simply fetch the service model directly
            var retUser = await _userService.GetUserByEmailAsync(email);

            // Check if the returned object is null or active without recursive mapping blocks
            return retUser != null;

            //var retUser = await _userService.GetUserByEmailAsync(email);
            //var user = _mapper.Map<UserAPIModel>(_mapper.Map<UserAPIModel>(retUser));
            //return user != null ? true : false;
        }       

        [HttpPost]
        // SECURITY FIX (2026-08-03): was [AllowAnonymous] - any unauthenticated caller could
        // create arbitrary users. Locked to Administrator. NOTE: this action's underlying
        // UserService.AddUserAsync currently writes to the legacy, non-Identity `User` table
        // (ApparelPro.Data/Models/Registration/User.cs), NOT AspNetUsers - a user "created" this
        // way cannot actually log in today. Flagged in the design doc (section 8); not fixed as
        // part of this security pass since it needs its own review, not a drive-by change.
        [ProducesResponseType(typeof(void), HttpStatusCodes.Created)]
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserAPIModel userAPIModel)
        {
            var userServiceModel = _mapper.Map<UserServiceModel>(userAPIModel);
            var createdUserServiceModel = await _userService.AddUserAsync(userServiceModel);
            var createdUserAPIModel = _mapper.Map<UserAPIModel>(createdUserServiceModel);
            return CreatedAtRoute(nameof(GetUserByEmailAsync), new { email = createdUserServiceModel.Email }, null); 
        }

        [HttpPut()]
        // SECURITY FIX (2026-08-03): was [AllowAnonymous] - any unauthenticated caller could
        // update any user's record by email. Locked to Administrator.
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        public async Task<IActionResult> UpdateUserAsync([FromQuery] string email, [FromBody] UpdateUserAPIModel updateUserAPIModel)
        {
            var userServiceModel = _mapper.Map<UpdateUserServiceModel>(updateUserAPIModel);
            try
            {
                await _userService.UpdateUserAsync(userServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(RegisterUserAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(UnauthorizedResult), HttpStatusCodes.Unauthorized)]
        [AllowAnonymous]
        [SwaggerOperation(Tags = new[] { "Authentication" },
            Summary = "'login (authenticate users).",
            Description = "Returns 200 - OK if called by an authenticated user regardless of its role(s).")
        ]        
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
        [SwaggerOperation(Tags = new[] { "Authentication" },
            Summary = "refresh token using existing token when token expires.",
            Description = "Returns 200 - OK if called by an authenticated user regardless of its role(s).")
        ]
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
        public async Task<IActionResult> RefreshToken1([FromBody] RegisteredUserAPIModel registeredUserAPIModel)
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

        // NEW (2026-08-03) - Users & Groups admin screen backing. Deliberately a new
        // "list-with-groups" route rather than fixing the existing "list" route in place -
        // that one's underlying service method reads a disconnected legacy table (see the
        // design doc, section 8) and needs its own review, not a drive-by change here.
        [HttpGet("list-with-groups")]
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        [ProducesResponseType(typeof(List<UserWithGroupsAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetUsersWithGroupsAsync()
        {
            var serviceModels = await _userService.GetIdentityUsersWithGroupsAsync();
            var apiModels = _mapper.Map<List<UserWithGroupsAPIModel>>(serviceModels);
            return Ok(apiModels);
        }

        [HttpPost("{userId}/groups/{groupId}")]
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        [ProducesResponseType(typeof(OkResult), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AssignUserToGroupAsync(string userId, string groupId)
        {
            try
            {
                await _userService.AssignUserToGroupAsync(userId, groupId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{userId}/groups/{groupId}")]
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        [ProducesResponseType(typeof(OkResult), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> RemoveUserFromGroupAsync(string userId, string groupId)
        {
            try
            {
                await _userService.RemoveUserFromGroupAsync(userId, groupId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

   public class Tokenizer
    {
        public string? token { get; set; }
    }
}
