using Muflone.Messages.Events;

namespace Muflone.Factories;

public interface IDomainEventHandlerFactory
{
	IDomainEventHandler<T> CreateDomainEventHandler<T>() where T : class, IDomainEvent;
}