using System.Collections.Generic;
using Muflone.Messages.Events;

namespace Muflone.Factories;

public interface IIntegrationEventHandlerFactory
{
	IEnumerable<IIntegrationEventHandler<T>> CreateIntegrationEventHandlers<T>()
		where T : class, IIntegrationEvent;
}