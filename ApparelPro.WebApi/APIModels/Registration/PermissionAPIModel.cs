namespace ApparelPro.WebApi.APIModels.Registration
{
    public class PermissionAPIModel
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
