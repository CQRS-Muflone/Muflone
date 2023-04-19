using System;
using Microsoft.Extensions.Logging;

namespace Muflone.Messages.Events;

public abstract class IntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
	where TEvent : IntegrationEvent
{
	protected readonly ILoggerFactory LoggerFactory;

	protected IntegrationEventHandler(ILoggerFactory loggerFactory)
	{
		LoggerFactory = loggerFactory;
	}

	public abstract void Handle(TEvent @event);

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

	~IntegrationEventHandler()
	{
		Dispose(false);
	}

	#endregion
}