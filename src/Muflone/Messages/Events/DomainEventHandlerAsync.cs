using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public abstract class DomainEventHandlerAsync<TEvent> : IDomainEventHandlerAsync<TEvent> where TEvent : class, IDomainEvent
{
	protected readonly ILogger Logger;
	private static readonly ActivitySource ActivitySource = new(OpenTelemetryConstants.ActivitySourceNames.DomainEventHandler, OpenTelemetryConstants.Version);


	protected DomainEventHandlerAsync(ILoggerFactory loggerFactory)
	{
		ArgumentNullException.ThrowIfNull(loggerFactory);
		Logger = loggerFactory.CreateLogger(typeof(DomainEventHandlerAsync<TEvent>));
	}

	public abstract Task ProcessAsync(TEvent @event, CancellationToken cancellationToken = default);

	public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(@event);
		cancellationToken.ThrowIfCancellationRequested();

		using var activity = ActivitySource.StartActivity(typeof(TEvent).Name, ActivityKind.Producer);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationPublish);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, typeof(TEvent).Name);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, @event.MessageId);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageCorrelationId, GetCorrelationId(@event));
		try
		{
			Logger.LogDebug("[Muflone.DomainEventHandlerAsync.HandleAsync] Handling domain event {EventType} with MessageId {MessageId}", typeof(TEvent).Name, @event.MessageId);
			await ProcessAsync(@event, cancellationToken);
			Logger.LogDebug("[Muflone.DomainEventHandlerAsync.HandleAsync] Successfully handled domain event {EventType} with MessageId {MessageId}", typeof(TEvent).Name, @event.MessageId);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "[Muflone.DomainEventHandlerAsync.HandleAsync] Error handling domain event {EventType} with MessageId {MessageId}", typeof(TEvent).Name, @event.MessageId);
			activity?.AddException(ex);
			throw;
		}
	}

	public Guid GetCorrelationId(TEvent @event)
	{
		return MessageHelpers.GetCorrelationId(@event);
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