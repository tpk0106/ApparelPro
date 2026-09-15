namespace ApparelPro.Data.DomainEvents
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    }
}
