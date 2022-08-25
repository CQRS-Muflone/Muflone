using System.Collections.Generic;
using Muflone.Messages.Events;

namespace Muflone.Factories;

public interface IDomainEventHandlerFactoryAsync
{
	IEnumerable<IDomainEventHandlerAsync<T>> CreateDomainEventHandlersAsync<T>() where T : class, IDomainEvent;
}