
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly IPasswordHasher<ApparelProUser> _passwordHasher;
        private readonly UserManager<ApparelProUser> _userManager;
        public UserService(ApparelProDbContext apparelProDbContext, IMapper mapper, ILookupConstants lookupConstants,
            ISecurityService securityService, UserIdentityDbContext userIdentityDbContext, IConfiguration configuration,
            IPasswordHasher<ApparelProUser> passwordHasher, UserManager<ApparelProUser> userManager)
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
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
            _lookupConstants = lookupConstants;
            _securityService = securityService;
            _userIdentityDbContext = userIdentityDbContext;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _userManager = userManager;
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
            // 1. Fetch and map identity parameters straight to your service model
            var userEntity = await _userIdentityDbContext.Users
                .Where(u => u.Email == email.ToLower())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (userEntity == null) return null;

            var userServiceModel = _mapper.Map<UserServiceModel>(userEntity);

            // 2. Fetch and map location properties from the second context
            if (userEntity.AddressId != null)
            {
                var addressEntity = await _apparelProDbContext.Addresses
                    .Where(a => a.AddressId == userEntity.AddressId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (addressEntity != null)
                {
                    // If your AutoMapper configuration maps Address -> UserServiceModel:
                    _mapper.Map(addressEntity, userServiceModel);
                }
            }

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
            Address newAddress = null;
            try
            {
                // 1. Instantiate and save the Address entry on its own connection first
                newAddress = new Address
                {
                    AddressId = Guid.NewGuid(),
                    StreetAddress = registerUserServiceModel.StreetAddress,
                    City = registerUserServiceModel.City,
                    PostCode = registerUserServiceModel.PostCode,
                    State = registerUserServiceModel.State,
                    CountryCode = registerUserServiceModel.CountryCode,
                    AddressType = registerUserServiceModel.AddressType.HasValue ? (AddressType)registerUserServiceModel.AddressType.Value : AddressType.Residential,
                    Default = registerUserServiceModel.Default
                };

                _apparelProDbContext.Addresses.Add(newAddress);
                await _apparelProDbContext.SaveChangesAsync(); // Saved and physically committed to SQL Server

                // 2. Map the User data and assign the generated AddressId Guid
                var user = _mapper.Map<ApparelProUser>(registerUserServiceModel);
                user.AddressId = newAddress.AddressId;
                user.RefreshTokenExpiry = null;
                user.Email = registerUserServiceModel?.Email?.ToLower();
                user.UserName = registerUserServiceModel?.Email?.ToLower();

                // 3. Verify user uniqueness before invoking creation engine
                if (await _userManager.FindByEmailAsync(user.Email) == null)
                {
                    var res1 = await _userManager.CreateAsync(user, registerUserServiceModel?.Password!);
                    if (!res1.Succeeded)
                    {
                        var errors = string.Join(", ", res1.Errors.Select(e => e.Description));
                        throw new Exception($"Identity Creation Failed: {errors}");
                    }

                    var res2 = await _userManager.AddToRolesAsync(user, new[] { "Merchandiser", "Merchandiser Manager", "Order Entry Operator" });
                    if (!res2.Succeeded)
                    {
                        throw new Exception("Assigning default roles failed.");
                    }
                }
                else
                {
                    throw new Exception("Email is already registered.");
                    //return BadRequest(new { message = "Email is already registered" });
                }

                var registeredUser = _mapper.Map<RegisterUserServiceModel>(user);
                return registeredUser;
            }
            catch (Exception)
            {
                // MANUAL ROLLBACK: If identity fields crash, safely remove the orphan address record
                if (newAddress != null)
                {
                    try
                    {
                        _apparelProDbContext.Addresses.Remove(newAddress);
                        await _apparelProDbContext.SaveChangesAsync();
                    }
                    catch
                    {
                        // Suppress rollback errors to ensure the primary identity crash exception gets outputted
                    }
                }
                throw;
            }
        }


        public async Task<RegisterUserServiceModel> RegisterAsync7(RegisterUserServiceModel registerUserServiceModel)
        {
            // 1. Open a transaction on the Identity context
            using var transaction = await _userIdentityDbContext.Database.BeginTransactionAsync();

            try
            {
                // 2. CRITICAL: Tell your Apparel Context to enlist in the exact same transaction space
                await _apparelProDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

                // 3. Instantiate and save the new Address entry
                var newAddress = new Address
                {
                    AddressId = Guid.NewGuid(),
                    StreetAddress = registerUserServiceModel.StreetAddress,
                    City = registerUserServiceModel.City,
                    PostCode = registerUserServiceModel.PostCode,
                    State = registerUserServiceModel.State,
                    CountryCode = registerUserServiceModel.CountryCode,
                    AddressType = registerUserServiceModel.AddressType.HasValue ? (AddressType)registerUserServiceModel.AddressType.Value : AddressType.Residential,
                    Default = true
                };

                _apparelProDbContext.Addresses.Add(newAddress);
                await _apparelProDbContext.SaveChangesAsync(); // Staged successfully in the shared transaction

                // 4. Map the User data and link the saved Address GUID
                var user = _mapper.Map<ApparelProUser>(registerUserServiceModel);
                user.AddressId = newAddress.AddressId;
                user.RefreshTokenExpiry = null;
                user.Email = registerUserServiceModel?.Email?.ToLower();
                user.UserName = registerUserServiceModel?.Email?.ToLower();

                // 5. Verify uniqueness and save the user
                if (await _userManager.FindByEmailAsync(user.Email) == null)
                {
                    // This will now pass because it can see the staged address inside the shared transaction space!
                    var res1 = await _userManager.CreateAsync(user, registerUserServiceModel?.Password!);
                    if (!res1.Succeeded)
                    {
                        var errors = string.Join(", ", res1.Errors.Select(e => e.Description));
                        throw new Exception($"Identity Creation Failed: {errors}");
                    }

                    var res2 = await _userManager.AddToRolesAsync(user, new[] { "Merchandiser", "Merchandiser Manager", "Order Entry Operator" });
                    if (!res2.Succeeded)
                    {
                        throw new Exception("Assigning default roles failed.");
                    }
                }
                else
                {
                    throw new Exception("Email is already registered.");
                }

                // 6. Commit everything to the physical database file simultaneously
                await transaction.CommitAsync();

                var registeredUser = _mapper.Map<RegisterUserServiceModel>(user);
                return registeredUser;
            }
            catch (Exception)
            {
                // If anything fails anywhere, the entire sequence rolls back completely
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RegisterUserServiceModel> RegisterAsync3(RegisterUserServiceModel registerUserServiceModel)
        {
            // 1. Create a cross-context transaction via the shared SQL connection
            using var transaction = await _userIdentityDbContext.Database.BeginTransactionAsync();

            try
            {

                // 2. Instantiate and save the new Address entry first
                var newAddress = new Address
                {
                    AddressId = Guid.NewGuid(), // Generate the unique identifier
                    StreetAddress = registerUserServiceModel.StreetAddress,
                    City = registerUserServiceModel.City,
                    PostCode = registerUserServiceModel.PostCode,
                    State = registerUserServiceModel.State,
                    CountryCode = registerUserServiceModel.CountryCode,
                    AddressType = registerUserServiceModel.AddressType.HasValue ? (AddressType)registerUserServiceModel.AddressType.Value : AddressType.Residential,
                    Default = true
                };

                _apparelProDbContext.Addresses.Add(newAddress);
                await _apparelProDbContext.SaveChangesAsync();

                // 3. Map the User data and link the saved Address GUID
                var user = _mapper.Map<ApparelProUser>(registerUserServiceModel);
                user.AddressId = newAddress.AddressId; // Inject the GUID reference
                user.RefreshTokenExpiry = null;
                user.Email = registerUserServiceModel?.Email?.ToLower();
                user.UserName = registerUserServiceModel?.Email?.ToLower(); // Identity requires a Username

                // 4. Verify uniqueness and commit user creation to Identity Store
                if (await _userManager.FindByEmailAsync(user.Email) == null)
                {
                    var res1 = await _userManager.CreateAsync(user, registerUserServiceModel?.Password!);
                    if (!res1.Succeeded)
                    {
                        var errors = string.Join(", ", res1.Errors.Select(e => e.Description));
                        throw new Exception($"Identity Creation Failed: {errors}");
                    }

                    var res2 = await _userManager.AddToRolesAsync(user, new[] { "Merchandiser", "Merchandiser Manager", "Order Entry Operator" });
                    if (!res2.Succeeded)
                    {
                        throw new Exception("Assigning default roles failed.");
                    }
                }
                else
                {
                    throw new Exception("Email is already registered.");
                }

                // 5. Commit everything to the physical database file simultaneously
                await transaction.CommitAsync();

                var registeredUser = _mapper.Map<RegisterUserServiceModel>(user);
                return registeredUser;
            }
            catch (Exception)
            {
                // Rollback ensures that if creating the User crashes, the Address record is safely deleted
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RegisterUserServiceModel> RegisterAsync2(RegisterUserServiceModel registerUserServiceModel)
        {
            var user = _mapper.Map<ApparelProUser>(registerUserServiceModel);
            user.RefreshTokenExpiry = null;
            if (await _userManager.FindByEmailAsync(user.Email!) == null)
            {
                var res1 = await _userManager.CreateAsync(user, registerUserServiceModel.Password!);
                var res2 = await _userManager.AddToRolesAsync(user, ["Merchandiser", "Merchandiser Manager", "Order Entry Operator"]);
            }
            await _apparelProDbContext.SaveChangesAsync();
            var registeredUser = _mapper.Map<RegisterUserServiceModel>(user);
            return registeredUser;
        }

        // ApparelProUser
        //public async Task<RegisterUserServiceModel> RegisterAsync(RegisterUserServiceModel registerUserServiceModel)
        //{
        //    // 18/02/25
        //    // create password hash

        //    var user = _mapper.Map<ApparelProUser>(registerUserServiceModel);
        //    var passwordHash = _passwordHasher.HashPassword(user,registerUserServiceModel.Password);
        //    user.PasswordHash = passwordHash;

        //    // end of hash

        //    user.Email = registerUserServiceModel.Email!.ToLower();

        //    _userIdentityDbContext.Users.Add(user);            
        //    await _userIdentityDbContext.SaveChangesAsync();

        //    var registeredUser = _mapper.Map<RegisterUserServiceModel>(user);
        //    return registeredUser;
        //}

        // this is not used in ApparelProUser but User
        public async Task<RegisterUserServiceModel> RegisterAsync1(RegisterUserServiceModel registerUserServiceModel)
        {
            var user = _mapper.Map<User>(registerUserServiceModel);
            using var hmac = new HMACSHA512();

            user.Email = registerUserServiceModel.Email.ToLower();
            user.PasswordSalt = hmac.Key;
            //  user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserServiceModel.Password));

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

            if (user == null) return null;

            // 2. Base mapping from user account metadata
            var registeredUserServiceModel = _mapper.Map<RegisteredUserServiceModel>(user);

            // 3. CROSS-CONTEXT FETCH: Extract full Address properties from Apparel Data Context
            if (user.AddressId.HasValue)
            {
                var addressData = await _apparelProDbContext.Addresses
                    .Where(a => a.AddressId == user.AddressId.Value)
                    .FirstOrDefaultAsync();

                if (addressData != null)
                {
                    // Inject Address values directly into the login return package
                    registeredUserServiceModel.StreetAddress = addressData.StreetAddress;
                    registeredUserServiceModel.City = addressData.City;
                    registeredUserServiceModel.State = addressData.State;
                    registeredUserServiceModel.PostCode = addressData.PostCode;
                    registeredUserServiceModel.CountryCode = addressData.CountryCode;
                }
            }
            ;

            // 4. Generate security payloads
            registeredUserServiceModel.Token = await CreateToken(registeredUserServiceModel);
            registeredUserServiceModel.RefreshToken = CreateRefreshToken();

            var duration = Convert.ToInt32(_configuration.GetRequiredSection("RefreshTokenExpiry").Value);
            registeredUserServiceModel.RefreshTokenExpiry = DateTime.Now.AddDays(duration);

            // 5. Update rotation key fields inside tracking tables
            user.RefreshToken = registeredUserServiceModel.RefreshToken;
            user.RefreshTokenExpiry = registeredUserServiceModel.RefreshTokenExpiry;

            await _userIdentityDbContext.SaveChangesAsync();

            return registeredUserServiceModel;
        }

        public string RefreshTokenUsingExistingToken(string token)
        {
            var refreshToken = _securityService.CreateRefreshToken();
            return refreshToken;
        }

        private async Task<string> CreateToken(RegisteredUserServiceModel registeredUserServiceModel)
        {
            return await _securityService.CreateTokenAsync(registeredUserServiceModel);
        }

        public async Task UpdateUserAsync(UpdateUserServiceModel updateUserServiceModel)
        {
            try
            {
                // 1. Fetch tracked User Context from Identity DB
                var retrievedUser = await _userManager.FindByEmailAsync(updateUserServiceModel.Email);

                //var retrievedUser = await _userManager.FindByEmailAsync(updateUserServiceModel.Email);
                if (retrievedUser == null)
                {
                    throw new Exception("User account not found.");
                }
                
                Guid? targetAddressId = updateUserServiceModel.AddressId;

                if (updateUserServiceModel.Address != null)
                {
                    // CASE A: User profile exists but doesn't have an address link yet (Create new Address)
                    if (targetAddressId == null || targetAddressId == Guid.Empty)
                    {
                        targetAddressId = Guid.NewGuid();

                        var newAddressEntity = new Address
                        {
                            AddressId = targetAddressId.Value,
                            StreetAddress = updateUserServiceModel.Address.StreetAddress,
                            City = updateUserServiceModel.Address.City,
                            PostCode = updateUserServiceModel.Address.PostCode,
                            State = updateUserServiceModel.Address.State,
                            CountryCode = updateUserServiceModel.Address.CountryCode,
                            AddressType = updateUserServiceModel.Address.AddressType.HasValue ?
                                (AddressType)updateUserServiceModel.Address.AddressType.Value : AddressType.Residential,
                            Default = updateUserServiceModel.Address.Default ?? true
                        };

                        _apparelProDbContext.Addresses.Add(newAddressEntity);
                        await _apparelProDbContext.SaveChangesAsync();
                    }

                    // CASE B: Address already exists (Update details in Business DB context)
                    else
                    {
                        var retrievedAddress = await _apparelProDbContext.Addresses
                            .Where(a => a.AddressId == targetAddressId)
                            .FirstOrDefaultAsync();

                        if (retrievedAddress != null)
                        {
                            retrievedAddress.StreetAddress = updateUserServiceModel.Address.StreetAddress;
                            retrievedAddress.City = updateUserServiceModel.Address.City;
                            retrievedAddress.PostCode = updateUserServiceModel.Address.PostCode;
                            retrievedAddress.State = updateUserServiceModel.Address.State;
                            retrievedAddress.CountryCode = updateUserServiceModel.Address.CountryCode;
                            retrievedAddress.AddressType = updateUserServiceModel.Address.AddressType.HasValue ?
                                (AddressType)updateUserServiceModel.Address.AddressType.Value : AddressType.Residential;

                            _apparelProDbContext.Addresses.Update(retrievedAddress);
                            await _apparelProDbContext.SaveChangesAsync();
                        }
                    }
                }

                // 2. Explicitly map parameters directly over tracked object context
                // This preserves your core security tokens and hashes safely!
                retrievedUser.AddressId = targetAddressId;
                retrievedUser.KnownAs = updateUserServiceModel.KnownAs;
                retrievedUser.DateOfBirth = updateUserServiceModel.DateOfBirth;
                retrievedUser.PhoneNumber = updateUserServiceModel.PhoneNumber;
                retrievedUser.ProfilePhoto = updateUserServiceModel.ProfilePhoto;
                retrievedUser.LastActive = DateTime.Now;

                if (updateUserServiceModel.Gender.HasValue)
                {
                    retrievedUser.Gender = (Gender)updateUserServiceModel.Gender.Value;
                }

                // 3. Save updates to Identity Database
                var result = await _userManager.UpdateAsync(retrievedUser);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"User update failed: {errors}");
                }

                //// 1. Instantiate and save the Address entry on its own connection first
                //var newAddress = new Address
                //{
                //    AddressId = updateUserServiceModel.AddressId.Value,
                //    StreetAddress = updateUserServiceModel?.Address.StreetAddress,
                //    City = updateUserServiceModel?.Address.City,
                //    PostCode = updateUserServiceModel?.Address.PostCode,
                //    State = updateUserServiceModel?.Address.State,
                //    CountryCode = updateUserServiceModel?.Address.CountryCode,
                //    AddressType = updateUserServiceModel.Address.AddressType.HasValue ?
                //        (AddressType)updateUserServiceModel.Address.AddressType.Value :
                //        AddressType.Residential,
                //    Default = updateUserServiceModel?.Address.Default
                //};

                //// new address, create it. 
                //if (updateUserServiceModel.Address != null && updateUserServiceModel.AddressId == null)
                //{
                //    newAddress.AddressId = Guid.NewGuid();
                //    _apparelProDbContext.Addresses.Add(newAddress);
                //    await _apparelProDbContext.SaveChangesAsync(); // Saved and physically committed to SQL Server
                //}
                //else if (updateUserServiceModel.Address != null && updateUserServiceModel.AddressId != null)
                //{
                //    var retrievedAddress = await _apparelProDbContext.Addresses
                //        .Where(address => address.AddressId == updateUserServiceModel.AddressId)
                //        .FirstOrDefaultAsync();
                //    if (retrievedAddress != null)
                //    {
                //        retrievedAddress.StreetAddress = updateUserServiceModel.Address.StreetAddress;
                //        retrievedAddress.City = updateUserServiceModel.Address.City;
                //        retrievedAddress.PostCode = updateUserServiceModel.Address.PostCode;
                //        retrievedAddress.State = updateUserServiceModel.Address.State;
                //        retrievedAddress.CountryCode = updateUserServiceModel.Address.CountryCode;
                //        retrievedAddress.AddressType = updateUserServiceModel.Address.AddressType.HasValue ?
                //            (AddressType)updateUserServiceModel.Address.AddressType.Value :
                //            AddressType.Residential;

                //        _apparelProDbContext.Addresses.Update(retrievedAddress);
                //        await _apparelProDbContext.SaveChangesAsync(); // Saved and physically committed to SQL Server
                //    }
                //}

                //// 2. Map the User data and assign the generated AddressId Guid
                //var user = _mapper.Map<ApparelProUser>(updateUserServiceModel);
                //user.AddressId = newAddress.AddressId;
                //user.RefreshTokenExpiry = null;
                //user.Email = updateUserServiceModel?.Email?.ToLower();
                //user.UserName = updateUserServiceModel?.Email?.ToLower();
                //user.KnownAs = updateUserServiceModel?.KnownAs;
                //user.DateOfBirth = updateUserServiceModel?.DateOfBirth;
                //user.PhoneNumber = updateUserServiceModel?.PhoneNumber;
                //user.ProfilePhoto = updateUserServiceModel.ProfilePhoto;

                //if (retrievedUser != null)
                //{
                //    var result =  await _userManager.UpdateAsync(user);
                //    if (!result.Succeeded)
                //    {
                //        throw new Exception("User update failed.");
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task UpdateUserAsync1(UpdateUserServiceModel updateUserServiceModel)
        {
            try
            {
                var userDbModel = await _userIdentityDbContext.Users
                .Where(u => u.Id == updateUserServiceModel.Id)
                .FirstOrDefaultAsync();

                if (userDbModel == null)
                {
                    throw new Exception("User account not found.");
                }
                // 2. Map updated top-level identity fields from DTO to the tracked user entity
                userDbModel.KnownAs = updateUserServiceModel.KnownAs;
                userDbModel.Gender = updateUserServiceModel.Gender;
                userDbModel.DateOfBirth = updateUserServiceModel.DateOfBirth;
                userDbModel.PhoneNumber = updateUserServiceModel.PhoneNumber;
                userDbModel.ProfilePhoto = updateUserServiceModel.ProfilePhoto;

                // 3. Search for an existing address entry using the assigned AddressId
                var existingAddress = await _apparelProDbContext.Addresses
                    .Where(address => address.AddressId == updateUserServiceModel.Address.AddressId)
                    .FirstOrDefaultAsync();

                if (existingAddress?.AddressId != null && updateUserServiceModel.Address != null)
                {
                    existingAddress.StreetAddress = updateUserServiceModel?.Address?.StreetAddress;
                    existingAddress.AddressType = updateUserServiceModel?.Address?.AddressType;
                    existingAddress.State = updateUserServiceModel?.Address?.State;
                    existingAddress.City = updateUserServiceModel?.Address?.City;
                    existingAddress.CountryCode = updateUserServiceModel?.Address?.CountryCode;
                    existingAddress.PostCode = updateUserServiceModel?.Address?.PostCode;

                    _apparelProDbContext.Addresses.Update(existingAddress);
                }
                else if (updateUserServiceModel.Address != null)
                {
                    // 4. Fallback execution path if the user profile didn't have an address yet

                    var newAddress = new Address
                    {
                        AddressId = Guid.NewGuid(),
                        City = updateUserServiceModel.Address.City,
                        Country = updateUserServiceModel.Address.Country,
                        PostCode = updateUserServiceModel.Address.PostCode,
                        AddressType = updateUserServiceModel.Address.AddressType,
                        StreetAddress = updateUserServiceModel.Address.StreetAddress,
                        Default = updateUserServiceModel.Address.Default,
                        State = updateUserServiceModel.Address.State,
                        CountryCode = updateUserServiceModel.Address.CountryCode
                    };
                    _apparelProDbContext.Addresses.Add(newAddress);

                    // Link the generated reference back to your user tracking metadata
                    userDbModel.AddressId = newAddress.AddressId;

                    // 5. Explicitly instruct BOTH distinct context engines to persist changes

                    _userIdentityDbContext.Users.Update(userDbModel);

                    await _apparelProDbContext.SaveChangesAsync();
                    await _userIdentityDbContext.SaveChangesAsync();

                }
            }
            catch (Exception e)
            {
                throw new Exception("Error !", e);
            }
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
