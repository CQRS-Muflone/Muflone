using System.Threading.Tasks;

namespace Muflone.Messages.Events
{
  public interface IIntegrationEventHandler
  {
  }

  public interface IIntegrationEventHandler<in T> : IIntegrationEventHandler where T : IIntegrationEvent
  {
    Task Handle(T @event);
  }
}