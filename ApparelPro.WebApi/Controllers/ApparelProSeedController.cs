using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService;
using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.APIModels.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ApparelPro.WebApi.Authorization;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // SECURITY FIX (2026-08-03): this controller had NO [Authorize] at all - anyone could hit
    // GET /api/ApparelProSeed and (re-)create the two hardcoded demo accounts below with their
    // hardcoded plaintext passwords. Those two passwords should still be treated as burned
    // (they're in git history) and rotated separately from this fix.
    //
    // Deliberately NOT a class-level [Authorize(Roles = "Administrator")]: LoadData is the
    // bootstrap path that creates the very first Administrator account on a brand-new database
    // (e.g. Production's Azure SQL database on first deploy), so it can't itself require an
    // Administrator to already be signed in - see LoadData's own self-limiting check below.
    // SeedPermissionsAsync has no such bootstrap need and carries its own method-level
    // [Authorize] instead.
    public class ApparelProSeedController : ControllerBase
    {
        private readonly UserManager<ApparelProUser> _userManager;
        private readonly SignInManager<UserAPIModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IConfiguration _configuration;
        private readonly IPermissionService _permissionService;
        private readonly IRolePermissionCache _rolePermissionCache;

        public ApparelProSeedController(UserManager<ApparelProUser> userManager, RoleManager<IdentityRole> roleManager, ApparelProDbContext apparelProDbContext, IConfiguration configuration, IPermissionService permissionService, IRolePermissionCache rolePermissionCache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _apparelProDbContext = apparelProDbContext;
            _configuration = configuration;
            _permissionService = permissionService;
            _rolePermissionCache = rolePermissionCache;
        }


        [HttpGet]
        // Self-limiting bootstrap endpoint (2026-08-03): reachable anonymously ONLY while zero
        // users hold the Administrator role in this database yet - the very first run against a
        // fresh database (e.g. right after `dotnet ef database update` on a new Azure SQL
        // database, which starts with zero users/roles). Once an Administrator exists, every
        // subsequent call requires the caller to already be an authenticated Administrator - this
        // check runs on every call, so there's no separate "lock it down after first use" step
        // and nothing to remember to flip back at deploy time.
        public async Task<ActionResult> LoadData()
        {
            var administratorRoleName = "Administrator";
            var administratorRoleAlreadyExists = await _roleManager.FindByNameAsync(administratorRoleName) != null;
            if (administratorRoleAlreadyExists)
            {
                var existingAdministrators = await _userManager.GetUsersInRoleAsync(administratorRoleName);
                if (existingAdministrators.Count > 0)
                {
                    var callerIsAuthenticatedAdministrator =
                        User?.Identity?.IsAuthenticated == true && User.IsInRole(administratorRoleName);
                    if (!callerIsAuthenticatedAdministrator)
                    {
                        return Forbid();
                    }
                }
            }

            var merchandiser = "Merchandiser";
            var orderEntryOperator = "Order Entry Operator";
            var merchandiserManager = "Merchandiser Manager";
            var inventory = "Inventory";
            var administrator = "Administrator";
            var storeManager = "Store Manager";
        
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
            if (await _roleManager.FindByNameAsync(storeManager) == null)
            {
                // Higher-authority role: gates access to Stock Adjustment Note (SAN),
                // which directly overwrites physical stock counts with no ceiling check
                // (see StockAdjustmentNoteService's class-level comment) — restricted to
                // this role plus Administrator rather than the broad Inventory/
                // Merchandiser/Order Entry Operator access every other note allows.
                await _roleManager.CreateAsync(new IdentityRole(storeManager));
            }

            // create a list to track the newly added users
            var userList = new List<ApparelProUser>();            
            var userRolesUpatedList = new List<ApparelProUser>();            
            var Merchandiser_Email = "tpk1966@yahoo.com";
            var Merchandiser_Phone = "0410251917";
            var Merchandiser_KnownAs = "Sampath";
            var Merchandiser_Password = "Thusith7291##";
            var userCreationErrors = new List<string>();

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

                var createResult = await _userManager.CreateAsync(userMerchandiser, Merchandiser_Password);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRolesAsync(userMerchandiser, [merchandiser, merchandiserManager]);
                    userList.Add(userMerchandiser);
                    var res = await _userManager.IsInRoleAsync(userMerchandiser, merchandiser);
                    Console.WriteLine("$userMerchandiser : {0} ", res);
                }
                else
                {
                    // Never attempt AddToRoleAsync on a user CreateAsync failed to persist —
                    // doing so previously caused an unhandled FK violation on AspNetUserRoles,
                    // since the in-memory user object still carries a constructor-generated Id
                    // that was never actually written to AspNetUsers.
                    userCreationErrors.Add($"{Merchandiser_Email}: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (await _userManager.FindByNameAsync(Merchandiser_Email) != null)
            {
                var _userMerchandiser = await _userManager.FindByNameAsync(Merchandiser_Email);
                var res = await _userManager.AddToRoleAsync(_userMerchandiser!,administrator);
                userRolesUpatedList.Add(_userMerchandiser!);
                Console.WriteLine("$_userMerchandiser : {0} ", res);

                // tpk1966@yahoo.com granted Store Manager access, per explicit request,
                // so this account can use Stock Adjustment Note.
                var storeManagerRes = await _userManager.AddToRoleAsync(_userMerchandiser!, storeManager);
                Console.WriteLine("$_userMerchandiser storeManager : {0} ", storeManagerRes);
            }

            var Stores_Email = "thusith@gmail.com";
            var Stores_Phone = "0411111917";
            var stores_KnownAs = "Sampi";
            var stores_Password = "fuckme1978*";
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

                var createResult = await _userManager.CreateAsync(userStores, stores_Password);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(userStores, inventory);
                    userList.Add(userStores);
                    var res = await _userManager.IsInRoleAsync(userStores, inventory);
                    Console.WriteLine("$userStores : {0} ", res);
                }
                else
                {
                    userCreationErrors.Add($"{Stores_Email}: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (userList.Count > 0)
                await _apparelProDbContext.SaveChangesAsync();
            return new JsonResult(new
            {
                Count = userList.Count,
                Users = userList,
                RolesCount = userRolesUpatedList.Count,
                RolesUpdated = userRolesUpatedList,
                Errors = userCreationErrors
            });
        }

        [HttpGet("seed-permissions")]
        // No bootstrap need here (permission-catalog seeding only makes sense once roles/admins
        // already exist) - always requires an authenticated Administrator.
        [Authorize(Roles = AccessPolicies.AdministratorOnly)]
        public async Task<ActionResult> SeedPermissionsAsync()
        {
            await _permissionService.SeedDefaultCatalogAsync();
            await _permissionService.RemoveObsoleteCatalogEntriesAsync();

            // FIXED (2026-08-07): this endpoint changes RolePermissions grants exactly like
            // PermissionsController.UpdateRolePermissionsAsync does, but was never invalidating
            // RolePermissionCache - a freshly-seeded permission (e.g. a brand-new policy like
            // trim-sheet-report) would silently keep denying every role for up to the cache's
            // 10-minute absolute expiration instead of taking effect immediately.
            _rolePermissionCache.Invalidate();

            return Ok(new { Message = "Permission catalog and default role grants seeded (idempotent - existing grants were not touched); obsolete catalog keys removed." });
        }
    }
}
