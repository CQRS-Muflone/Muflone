using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Persistence;

namespace Muflone.Messages.Commands
{
	public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
	{
		protected readonly IRepository Repository;
		protected readonly ILogger Logger;

		protected CommandHandler(IRepository repository, ILoggerFactory loggerFactory)
		{
			Repository = repository ?? throw new ArgumentNullException(nameof(repository));
			Logger = loggerFactory?.CreateLogger(GetType()) ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public abstract Task Handle(TCommand command);

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

		~CommandHandler()
		{
			Dispose(false);
		}
	}
}