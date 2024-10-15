using Microsoft.Extensions.Logging;
using System;

namespace Muflone.Messages.Events;

public abstract class IntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
	where TEvent : IntegrationEvent
{
	protected readonly ILoggerFactory LoggerFactory;
	protected readonly ILogger Logger;

	protected IntegrationEventHandler(ILoggerFactory loggerFactory)
	{
		LoggerFactory = loggerFactory;
		Logger = loggerFactory.CreateLogger(GetType());
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