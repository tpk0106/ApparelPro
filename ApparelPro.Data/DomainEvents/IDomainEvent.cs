namespace ApparelPro.Data.DomainEvents
{
    // Marker interface only - carries no behavior. Its only job is to let the
    // dispatcher and handlers agree on "this is a thing that happened",
    // without forcing every event to share a base class.
    public interface IDomainEvent { }
}
