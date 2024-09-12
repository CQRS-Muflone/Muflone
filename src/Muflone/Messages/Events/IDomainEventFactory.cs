using System;

namespace Muflone.Messages.Events;

public interface IDomainEventFactory
{
	IDomainEventHandler CreateDomainEventHandler(Type handlerType);
}