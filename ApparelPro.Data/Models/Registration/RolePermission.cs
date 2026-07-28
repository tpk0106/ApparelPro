namespace ApparelPro.Data.Models.Registration
{
    /// <summary>
    /// The many-to-many bridge between ASP.NET Identity roles (AspNetRoles) and the
    /// Permission catalog. Granting/revoking a role's access to a section is an
    /// insert/delete of a row here - a data change, not a code change.
    /// </summary>
    public class RolePermission
    {
        public int Id { get; set; }

        /// <summary>FK -> AspNetRoles.Id (IdentityRole's string key).</summary>
        public string RoleId { get; set; } = string.Empty;

        public int PermissionId { get; set; }

        public Permission? Permission { get; set; }
    }
}
