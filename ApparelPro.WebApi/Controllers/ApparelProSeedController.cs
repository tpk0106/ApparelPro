using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.APIModels.Registration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApparelProSeedController : ControllerBase
    {
        private readonly UserManager<ApparelProUser> _userManager;
        private readonly SignInManager<UserAPIModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IConfiguration _configuration;

        public ApparelProSeedController(UserManager<ApparelProUser> userManager, RoleManager<IdentityRole> roleManager, ApparelProDbContext apparelProDbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _apparelProDbContext = apparelProDbContext;
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<ActionResult> LoadData()
        {
            var merchandiser = "Merchandiser";
            var orderEntryOperator = "Order Entry Operator";
            var merchandiserManager = "Merchandiser Manager";
            var inventory = "Inventory";
            var administrator = "Administrator";
        
            // create the default roles (if they don't exist yet)
            if (await _roleManager.FindByNameAsync(merchandiser) == null)
            {               
                await _roleManager.CreateAsync(new IdentityRole(merchandiser));
            }
            if( await _roleManager.FindByNameAsync(orderEntryOperator) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(orderEntryOperator));
            }
            if(await _roleManager.FindByNameAsync(merchandiserManager)==null)
            {
                await _roleManager.CreateAsync(new IdentityRole(merchandiserManager));
            }
            if(await _roleManager.FindByNameAsync(inventory) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(inventory));
            }
            if (await _roleManager.FindByNameAsync(administrator) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(administrator));
            }

            // create a list to track the newly added users
            var userList = new List<ApparelProUser>();            
            var userRolesUpatedList = new List<ApparelProUser>();            
            var Merchandiser_Email = "tpk0106@yahoo.com";
            var Merchandiser_Phone = "041001917";
            var Merchandiser_KnownAs = "Sampath";
            var Merchandiser_Password = "thusith7291##";

            var userMerchandiser = new UserAPIModel();
            if (await _userManager.FindByNameAsync(Merchandiser_Email) == null)
            {                
                userMerchandiser.Email = Merchandiser_Email;
                userMerchandiser.UserName = Merchandiser_Email;
                userMerchandiser.PhoneNumber = Merchandiser_Phone;
                userMerchandiser.Gender = Data.Models.Registration.Gender.Male;
                userMerchandiser.KnownAs = Merchandiser_KnownAs;
                userMerchandiser.EmailConfirmed = true;
                userMerchandiser.LockoutEnabled = false;

                await _userManager.CreateAsync(userMerchandiser, Merchandiser_Password);
                await _userManager.AddToRolesAsync(userMerchandiser, [merchandiser, merchandiserManager]);
                
                userList.Add(userMerchandiser);
                var res = await _userManager.IsInRoleAsync(userMerchandiser, merchandiser);
                Console.WriteLine("$userMerchandiser : {0} ", res);
            }

            if (await _userManager.FindByNameAsync(Merchandiser_Email) != null)
            {
                var _userMerchandiser = await _userManager.FindByNameAsync(Merchandiser_Email);
                var res = await _userManager.AddToRoleAsync(_userMerchandiser!,administrator);
                userRolesUpatedList.Add(_userMerchandiser!);
                Console.WriteLine("$_userMerchandiser : {0} ", res);
            }

            var Stores_Email = "thusith@gmail.com";
            var Stores_Phone = "0411111917";
            var stores_KnownAs = "Sampi";
            var stores_Password = "*******";
            var userStores = new ApparelProUser();
            if (await _userManager.FindByNameAsync(Stores_Email) == null)
            {               
                userStores.Email = Stores_Email;
                userStores.UserName = Stores_Email;
                userStores.PhoneNumber = Stores_Phone;
                userStores.Gender = Data.Models.Registration.Gender.Female;
                userStores.KnownAs = stores_KnownAs;

                userStores.EmailConfirmed = true;
                userStores.LockoutEnabled = false;

                await _userManager.CreateAsync(userStores, stores_Password);
                await _userManager.AddToRoleAsync(userStores, inventory);

                userList.Add(userStores);

                var res = await _userManager.IsInRoleAsync(userStores, inventory);
                Console.WriteLine("$userStores : {0} ", res);
            }

            if (userList.Count > 0)
                await _apparelProDbContext.SaveChangesAsync();
            return new JsonResult(new
            {
                Count = userList.Count,
                Users = userList,
                RolesCount = userRolesUpatedList.Count,
                RolesUpdated = userRolesUpatedList
            });
        }
    }
}
