using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Events;

namespace Muflone;

public interface IEventBus
{
	Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
}