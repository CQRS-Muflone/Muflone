using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.Persistence;

namespace Muflone.Messages.Commands
{
	public abstract class DomainEventHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
	{
		protected readonly IPersister Persister;
		protected readonly ILoggerFactory LoggerFactory;

		protected DomainEventHandler(IPersister persister, ILoggerFactory loggerFactory)
		{
			Persister = persister;
			LoggerFactory = loggerFactory;
		}

		public abstract Task Handle(TEvent command);

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

		~DomainEventHandler()
		{
			Dispose(false);
		}
	}
}
