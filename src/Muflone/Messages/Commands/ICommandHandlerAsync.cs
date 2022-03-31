using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Commands;

public interface ICommandHandlerAsync : IDisposable
{
}

public interface ICommandHandlerAsync<in TCommand> : ICommandHandlerAsync where TCommand : class, ICommand
{
	Task HandleAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken));
}