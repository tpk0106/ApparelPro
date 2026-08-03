using System.Collections.Generic;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService
{
    public class CopyMaterialsFromStyleResultServiceModel
    {
        public int CopiedCount { get; set; }
        public int SkippedCount { get; set; }

        // Human-readable descriptions of items skipped because they already existed at the
        // target Style, so the UI can tell the merchandiser exactly what was left untouched.
        public List<string> SkippedItemDescriptions { get; set; } = new List<string>();
    }
}
