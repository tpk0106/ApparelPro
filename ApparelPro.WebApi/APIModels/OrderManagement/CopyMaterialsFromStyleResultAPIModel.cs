using System.Collections.Generic;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CopyMaterialsFromStyleResultAPIModel
    {
        public int CopiedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> SkippedItemDescriptions { get; set; } = new List<string>();
    }
}
