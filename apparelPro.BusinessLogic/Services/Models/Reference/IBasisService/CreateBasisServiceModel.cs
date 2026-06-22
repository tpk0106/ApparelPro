using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IBasisService
{
    public class CreateBasisServiceModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public bool ValueAdd { get; set; }
    }
}
