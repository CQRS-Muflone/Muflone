using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone;

public interface IEventBus
{
	Task PublishAsync<T>(IMessage @event) where T : class, IDomainEvent;
}