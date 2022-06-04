using System.Threading.Tasks;
using Muflone.Messages;

namespace Muflone;

public interface IEventBus
{
	Task PublishAsync(IMessage @event);
}