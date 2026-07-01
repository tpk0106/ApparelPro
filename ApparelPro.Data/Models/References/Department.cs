using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Models.References
{
    public class Department
    {
        public string DepartmentCode { get; set; } = null!; // xdept (e.g., "CUT", "SEW", "STR")
        public string Name { get; set; } = null!;
    }
}
