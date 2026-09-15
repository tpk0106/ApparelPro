namespace ApparelPro.Data.DomainEvents
{
    public interface IHasDomainEvents
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }

    // Inherit this in any entity that should be able to announce something
    // about itself. Protected Raise() is the only way to add an event - an
    // outside caller can read DomainEvents but never inject one, so an
    // entity's own state changes are always what trigger its own events.
    public abstract class EntityWithDomainEvents : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
