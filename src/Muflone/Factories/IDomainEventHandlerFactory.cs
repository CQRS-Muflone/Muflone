using Muflone.Messages.Events;
using System.Collections.Generic;

namespace Muflone.Factories;

public interface IDomainEventHandlerFactory
{
  IEnumerable<IDomainEventHandler<T>> CreateDomainEventHandlers<T>() where T : class, IDomainEvent;
}