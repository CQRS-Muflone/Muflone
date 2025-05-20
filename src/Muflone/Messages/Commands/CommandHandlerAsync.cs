using Microsoft.Extensions.Logging;
using Muflone.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Commands;

public abstract class CommandHandlerAsync<TCommand>(IRepository repository, ILoggerFactory loggerFactory) 
	: ICommandHandlerAsync<TCommand> where TCommand : class, ICommand
{
	protected readonly IRepository Repository = repository;
	protected readonly ILogger Logger = loggerFactory.CreateLogger(typeof(CommandHandlerAsync<TCommand>));

	public abstract Task HandleAsync(TCommand command, CancellationToken cancellationToken = new());

	#region Dispose

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
		}
	}


	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}


	~CommandHandlerAsync()
	{
		Dispose(false);
	}

	#endregion
}