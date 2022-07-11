using System.Collections.Generic;
using Muflone.Messages.Events;

namespace Muflone.Factories;

public interface IIntegrationEventHandlerFactoryAsync
{
	IIntegrationEventHandlerAsync<T> CreateIntegrationEventHandlerAsync<T>() where T : class, IIntegrationEvent;

	IEnumerable<IIntegrationEventHandlerAsync<T>> CreateIntegrationEventHandlersAsync<T>()
		where T : class, IIntegrationEvent;
}