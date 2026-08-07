using apparelPro.BusinessLogic.Services.Models.Registration.IGroupService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IGroupService
    {
        // List of every Group (AspNetRoles row) with a live member count, for the
        // Users & Groups admin screen's Groups panel.
        Task<List<GroupServiceModel>> GetGroupsAsync();

        // Creates a new Group (a plain AspNetRoles row - "Group" is just this app's
        // product-facing name for a Role, per the design doc - no separate Group table).
        Task<GroupServiceModel> CreateGroupAsync(string name);

        // Blocked while the group still has members (user decision, 2026-08-03) - throws
        // InvalidOperationException naming the remaining members instead of silently
        // dropping their access. RolePermissions grants cascade-delete automatically at
        // the DB level (RolePermissionConfig's OnDelete(DeleteBehavior.Cascade) on RoleId).
        Task DeleteGroupAsync(string groupId);
    }
}
