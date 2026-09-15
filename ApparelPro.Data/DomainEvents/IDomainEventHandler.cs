namespace ApparelPro.Data.DomainEvents
{
    // One handler per (event type, reaction). Multiple handlers can exist for
    // the same event - e.g. a PO being raised could have one handler that
    // updates the pipeline and a separate one that sends a notification,
    // added independently without either knowing about the other.
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent, CancellationToken ct);
    }
}
