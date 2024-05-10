using Muflone.Messages.Events;
using System.Collections.Generic;

namespace Muflone.Factories;

public interface IIntegrationEventHandlerFactoryAsync
{
  IEnumerable<IIntegrationEventHandlerAsync<T>> CreateIntegrationEventHandlersAsync<T>()
    where T : class, IIntegrationEvent;
}