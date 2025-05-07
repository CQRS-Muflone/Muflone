using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public abstract class DomainEventHandlerAsync<TEvent>(ILoggerFactory loggerFactory) 
	: IDomainEventHandlerAsync<TEvent> where TEvent : class, IDomainEvent
{
	protected readonly ILoggerFactory LoggerFactory = loggerFactory;

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

	~DomainEventHandlerAsync()
	{
		Dispose(false);
	}

	#endregion
}