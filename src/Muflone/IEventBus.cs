using Muflone.Messages.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone;

public interface IEventBus
{
  Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
}