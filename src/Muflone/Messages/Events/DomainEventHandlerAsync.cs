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

	public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);

	async Task IMessageHandlerAsync<TEvent>.HandleAsync(TEvent @event, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(@event);
		cancellationToken.ThrowIfCancellationRequested();

		var activityName = typeof(TEvent).Name;
		Activity? activity = OpenTelemetryMessageHelpers.TryExtractParentContext(@event, out var parentContext)
			? ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext)
			: ActivitySource.StartActivity(activityName, ActivityKind.Consumer);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationConsume);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, typeof(TEvent).Name);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, @event.MessageId.ToString());
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageCorrelationId, GetCorrelationId(@event).ToString());

			Logger.LogDebug("[Muflone.DomainEventHandlerAsync.HandleAsync] Handling domain event {EventType} with MessageId {MessageId}", typeof(TEvent).Name, @event.MessageId);
			await HandleAsync(@event, cancellationToken);
			Logger.LogDebug("[Muflone.DomainEventHandlerAsync.HandleAsync] Successfully handled domain event {EventType} with MessageId {MessageId}", typeof(TEvent).Name, @event.MessageId);

			activity?.SetStatus(ActivityStatusCode.Ok);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "[Muflone.DomainEventHandlerAsync.HandleAsync] Error handling domain event {EventType} with MessageId {MessageId}", typeof(TEvent).Name, @event.MessageId);
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
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
