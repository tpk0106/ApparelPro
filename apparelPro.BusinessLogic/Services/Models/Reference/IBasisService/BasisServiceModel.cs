using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IBasisService
{
    public  class BasisServiceModel {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool ValueAdd { get; set; }
    }
}
