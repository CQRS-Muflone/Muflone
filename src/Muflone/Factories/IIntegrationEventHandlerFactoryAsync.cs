using System.Collections.Generic;
using Muflone.Messages.Events;

namespace Muflone.Factories;

public interface IIntegrationEventHandlerFactoryAsync
{
	IEnumerable<IIntegrationEventHandlerAsync<T>> CreateIntegrationEventHandlersAsync<T>()
		where T : class, IIntegrationEvent;
}