using Muflone.Messages.Events;
using System.Collections.Generic;

namespace Muflone.Factories;

public interface IDomainEventHandlerFactoryAsync
{
	IEnumerable<IDomainEventHandlerAsync<T>> CreateDomainEventHandlersAsync<T>() where T : class, IDomainEvent;
}