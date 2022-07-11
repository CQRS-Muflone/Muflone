using System;

namespace Muflone.Messages.Events;

public interface IDomainEventHandler : IDisposable
{
}

public interface IDomainEventHandler<in TEvent> : IDomainEventHandler where TEvent : class, IDomainEvent
{
	void Handle(TEvent @event);
}