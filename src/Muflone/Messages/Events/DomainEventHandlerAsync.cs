using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public abstract class DomainEventHandlerAsync<TEvent> : IDomainEventHandlerAsync<TEvent> where TEvent : class, IDomainEvent
{
	protected readonly ILogger Logger;

	protected DomainEventHandlerAsync(ILoggerFactory loggerFactory)
	{
		Logger = loggerFactory.CreateLogger(typeof(DomainEventHandlerAsync<TEvent>));
	}

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

	~DomainEventHandlerAsync()
	{
		Dispose(false);
	}

	#endregion
}