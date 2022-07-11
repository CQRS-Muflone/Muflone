using System.Collections.Generic;
using Muflone.Messages.Events;

namespace Muflone.Factories;

public interface IDomainEventHandlerFactoryAsync
{
	IDomainEventHandlerAsync<T> CreateDomainEventHandlerAsync<T>() where T : class, IDomainEvent;
	IEnumerable<IDomainEventHandlerAsync<T>> CreateDomainEventHandlersAsync<T>() where T : class, IDomainEvent;
}