using System;
using System.Threading.Tasks;

namespace Muflone.Messages.Events
{
  public interface IIntegrationEventHandler: IDisposable
  {
  }

  public interface IIntegrationEventHandler<in TEvent> : IIntegrationEventHandler where TEvent : IIntegrationEvent
  {
    Task Handle(TEvent @event);
  }
}