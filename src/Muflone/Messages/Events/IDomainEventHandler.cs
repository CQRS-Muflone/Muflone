namespace Muflone.Messages.Events;

public interface IDomainEventHandler : IMessageHandler
{
}

public interface IDomainEventHandler<in TEvent> : IDomainEventHandler where TEvent : class, IDomainEvent
{
}