using ApparelPro.Shared.LookupConstants.ApparelProContext;

namespace ApparelPro.Shared.LookupConstants
{
    public interface ILookupConstants
    {
        IDatabaseLookupConstants ApparelPro { get; }
        IReferenceLookup Reference { get; }
        IParams Params { get; }
        IFilters Filters { get; }
    }

    public interface IDatabaseLookupConstants
    {
    }

    public interface IReferenceLookup
    {
       AddressType AddresssType { get; }
    }

    public interface IParams
    {
        Page PageParams { get; }
    }

    public interface IFilters
    {
        Page PageFilters { get; }
    }
}
