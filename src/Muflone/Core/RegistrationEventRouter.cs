using System;
using System.Collections.Generic;

namespace Muflone.Core
{
  public class RegistrationEventRouter : IRouteEvents
  {
    private readonly IDictionary<Type, Action<object>> handlers = new Dictionary<Type, Action<object>>();

    private IAggregate regsitered;

    public virtual void Register<T>(Action<T> handler)
    {
      handlers[typeof(T)] = @event => handler((T)@event);
    }

    public virtual void Register(IAggregate aggregate)
    {
      regsitered = aggregate ?? throw new ArgumentNullException("aggregate");
    }

    public virtual void Dispatch(object eventMessage)
    {
      if (!handlers.TryGetValue(eventMessage.GetType(), out var handler))
        regsitered.ThrowHandlerNotFound(eventMessage);

      handler(eventMessage);
    }
  }
}