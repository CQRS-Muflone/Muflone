using Muflone.Messages.Events;
using System.Collections.Generic;

namespace Muflone.Factories;

public interface IIntegrationEventHandlerFactory
{
	IEnumerable<IIntegrationEventHandler<T>> CreateIntegrationEventHandlers<T>()
		where T : class, IIntegrationEvent;
}