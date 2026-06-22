using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService
{
    public class SupplierLookupServiceModel
    {
        public int SupplierCode { get; set; } // Matches your database primary key integer
        public string Name { get; set; } = string.Empty;
    }
}
