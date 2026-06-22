using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService
{
    public class StyleDimensionsLookupServiceModel
    {
        public List<string> Colors { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();
    }
}
