using apparelPro.BusinessLogic.Services.Models.Registration.IGroupService;
using ApparelPro.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Registration
{
    public class GroupService : IGroupService
    {
        private readonly UserIdentityDbContext _userIdentityDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GroupService(UserIdentityDbContext userIdentityDbContext, RoleManager<IdentityRole> roleManager)
        {
            _userIdentityDbContext = userIdentityDbContext ?? throw new ArgumentNullException(nameof(userIdentityDbContext));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<List<GroupServiceModel>> GetGroupsAsync()
        {
            // Left-join AspNetRoles -> AspNetUserRoles (GroupJoin) so a group with zero members
            // still appears in the list with MemberCount = 0, instead of disappearing entirely.
            var groups = await _userIdentityDbContext.Roles
                .GroupJoin(
                    _userIdentityDbContext.UserRoles,
                    role => role.Id,
                    userRole => userRole.RoleId,
                    (role, memberRoles) => new GroupServiceModel
                    {
                        Id = role.Id,
                        Name = role.Name ?? string.Empty,
                        MemberCount = memberRoles.Count()
                    })
                .OrderBy(group => group.Name)
                .ToListAsync();

            return groups;
        }

        public async Task<GroupServiceModel> CreateGroupAsync(string name)
        {
            var trimmedName = (name ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmedName))
            {
                throw new InvalidOperationException("Group name cannot be empty.");
            }

            if (await _roleManager.RoleExistsAsync(trimmedName))
            {
                throw new InvalidOperationException($"A group named '{trimmedName}' already exists.");
            }

            var role = new IdentityRole(trimmedName);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not create group: {errors}");
            }

            return new GroupServiceModel { Id = role.Id, Name = role.Name!, MemberCount = 0 };
        }

        public async Task DeleteGroupAsync(string groupId)
        {
            var role = await _roleManager.FindByIdAsync(groupId);
            if (role == null)
            {
                throw new InvalidOperationException("Group not found.");
            }

            // Inner-join AspNetUserRoles -> AspNetUsers, filtered to this role, to name the
            // remaining members in the error message below.
            var memberEmails = await _userIdentityDbContext.UserRoles
                .Where(userRole => userRole.RoleId == role.Id)
                .Join(
                    _userIdentityDbContext.Users,
                    userRole => userRole.UserId,
                    user => user.Id,
                    (userRole, user) => user.Email)
                .ToListAsync();

            if (memberEmails.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete group '{role.Name}': it still has {memberEmails.Count} member(s) ({string.Join(", ", memberEmails)}). Remove them from the group first.");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not delete group: {errors}");
            }
        }
    }
}
