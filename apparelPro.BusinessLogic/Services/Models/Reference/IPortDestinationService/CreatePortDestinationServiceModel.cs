using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService
{
    public class CreatePortDestinationServiceModel
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public string DestinationName { get; set; }
    }
}
