using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public abstract class IntegrationEventHandlerAsync<TEvent> : IIntegrationEventHandlerAsync<TEvent>
	where TEvent : IntegrationEvent
{
	protected readonly ILoggerFactory LoggerFactory;

	protected IntegrationEventHandlerAsync(ILoggerFactory loggerFactory)
	{
		LoggerFactory = loggerFactory;
	}

	public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);

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

	~IntegrationEventHandlerAsync()
	{
		Dispose(false);
	}

	#endregion
}