using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.Messages.Events;
using OpenTelemetry.Trace;

namespace Muflone.Persistence;

/// <summary>
/// Decorator that wraps an IEventBus implementation with OpenTelemetry producer spans.
/// </summary>
public sealed class InstrumentedEventBus : IEventBus
{
	private readonly IEventBus _inner;
	private static readonly ActivitySource ActivitySource = new(
		OpenTelemetryConstants.ActivitySourceNames.EventBus,
		OpenTelemetryConstants.Version);

	public InstrumentedEventBus(IEventBus inner)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
	}

	public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
	{
		var activityName = $"{typeof(T).Name} publish";
		Activity? activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationPublish);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, typeof(T).Name);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, @event.MessageId.ToString());

			// Re-inject trace context so the event carries the producer span's trace context
			OpenTelemetryMessageHelpers.InjectTraceContext(@event);

			await _inner.PublishAsync(@event, cancellationToken);

			activity?.SetStatus(ActivityStatusCode.Ok);
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
		}
	}
}
