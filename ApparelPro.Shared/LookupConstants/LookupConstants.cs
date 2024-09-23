namespace ApparelPro.Shared.LookupConstants.ApparelProContext
{
    public class LookupConstants : ILookupConstants
    {
        public IDatabaseLookupConstants ApparelPro { get; } = new DatabaseLookupConstants();
        public IReferenceLookup Reference { get; } = new ReferenceLookupConstants();
        public IParams Params { get; } = new Params();
        public IFilters Filters { get; } = new Filters();        
    }

    public class DatabaseLookupConstants : IDatabaseLookupConstants
    {
    }

    public class ReferenceLookupConstants : IReferenceLookup
    {
        public AddressType AddresssType { get; } = new AddressType();
    }

    public class Params : IParams
    {
        public Page PageParams { get; } = new Page();
    }

    public class Filters:IFilters
    {
        public Page PageFilters { get; } = new Page();
    }
}
