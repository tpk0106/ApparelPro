using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService
{
    public class UpdateFeatureServiceModel
    {
        public int Id { get; set; }
        public string? Description { get; set; } = null;
    }
}
