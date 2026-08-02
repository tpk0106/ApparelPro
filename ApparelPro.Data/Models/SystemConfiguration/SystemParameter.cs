namespace ApparelPro.Data.Models.SystemConfiguration
{
    public class SystemParameter
    {
        public string ParameterKey { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }

        // Drives the Settings screen's UI: which category tab this parameter appears
        // under, and which control renders for it (Boolean -> toggle, Color -> swatch
        // picker, Number -> slider/numeric input, Text -> text field, Select -> dropdown).
        public string Category { get; set; } = "General";
        public string DataType { get; set; } = "Text";

        // For DataType = "Select" only: comma-separated "label:value" pairs, e.g.
        // "10 rows:10,25 rows:25,50 rows:50". Null/unused for every other DataType.
        public string? Options { get; set; }
    }
}
