namespace apparelPro.BusinessLogic.Services.Models.SystemConfiguration.ISystemParameterService
{
    public class SystemParameterServiceModel
    {
        public string ParameterKey { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
        public string Category { get; set; } = "General";
        public string DataType { get; set; } = "Text";
        public string? Options { get; set; }
    }
}
