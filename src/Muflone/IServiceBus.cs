using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Commands;

namespace Muflone;

public interface IServiceBus
{
	Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
}