using Muflone.Messages.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Persistence;

public interface IServiceBus
{
	Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
}