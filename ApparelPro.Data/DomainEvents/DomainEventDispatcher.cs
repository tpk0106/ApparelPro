using Microsoft.Extensions.DependencyInjection;

namespace ApparelPro.Data.DomainEvents
{
    // Resolves every IDomainEventHandler<T> registered for the event's exact
    // runtime type via the DI container, and awaits them in order. This is
    // the one place that knows both "events" and "handlers" exist - nothing
    // else in the app needs to know how dispatch works.
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        public DomainEventDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = (_serviceProvider.GetServices(handlerType) as System.Collections.IEnumerable)?
                .Cast<object>().ToList() ?? new List<object>();

            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod("HandleAsync")!;
                await (Task)handleMethod.Invoke(handler, new object[] { domainEvent, ct })!;
            }
        }
    }
}
