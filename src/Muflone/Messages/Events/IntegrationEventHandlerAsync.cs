using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public abstract class IntegrationEventHandlerAsync<TEvent> : IIntegrationEventHandlerAsync<TEvent>
	where TEvent : IntegrationEvent
{
	public abstract Task HandleAsync(TEvent @event,
		CancellationToken cancellationToken = default(CancellationToken));

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