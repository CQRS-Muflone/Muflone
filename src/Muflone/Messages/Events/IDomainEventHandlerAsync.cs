namespace Muflone.Messages.Events;

public interface IDomainEventHandlerAsync : IMessageHandlerAsync
{
}

public interface IDomainEventHandlerAsync<TEvent> : IDomainEventHandlerAsync, IMessageHandlerAsync<TEvent>
    where TEvent : class, IDomainEvent
{
}