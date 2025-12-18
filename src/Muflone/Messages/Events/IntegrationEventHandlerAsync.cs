using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public abstract class IntegrationEventHandlerAsync<TEvent>(ILoggerFactory loggerFactory) : IIntegrationEventHandlerAsync<TEvent> where TEvent : IntegrationEvent
{
	protected readonly ILoggerFactory LoggerFactory = loggerFactory;

	public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
	
	public Guid GetCorrelationId(TEvent @event)
	{
		@event.UserProperties.TryGetValue(HeadersNames.CorrelationId, out var correlationId);
		return correlationId != null ? 
			Guid.Parse(correlationId.ToString()!) 
			: Guid.Empty;
	}

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