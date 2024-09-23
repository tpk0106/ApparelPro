using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService
{
    public class BuyerResult
    {
        public Buyer Buyer { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
