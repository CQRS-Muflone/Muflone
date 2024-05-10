using System;

namespace Muflone.Messages.Events;

public interface IIntegrationEventHandler : IDisposable
{
}

public interface IIntegrationEventHandler<in T> : IIntegrationEventHandler where T : IIntegrationEvent
{
  void Handle(T @event);
}