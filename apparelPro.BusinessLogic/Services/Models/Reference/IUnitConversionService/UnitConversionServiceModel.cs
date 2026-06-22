using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IUnitConversionService
{
    public class UnitConversionServiceModel
    {
        public string FromUnit { get; set; }
        public string ToUnit { get; set; }
        public decimal? Measure { get; set; }
    }
}
