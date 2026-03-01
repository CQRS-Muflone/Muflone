//using Muflone.Messages.Commands;
//using Muflone.Messages.Events;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using static Muflone.Messages.OpenTelemetryConstants;

//namespace Muflone.Messages;

///// <summary>
///// Extension methods for OpenTelemetry tracing with Muflone messages
///// </summary>
//public static class MufloneTracingExtensions
//{
//	/// <summary>
//	/// Injects the current trace context into the message's UserProperties using W3C Trace Context standard
//	/// </summary>
//	/// <param name="message">The message to inject trace context into</param>
//	public static void InjectTraceContext(this IMessage message)
//	{
//		ArgumentNullException.ThrowIfNull(message);

//		var activity = Activity.Current;
//		if (activity == null || string.IsNullOrEmpty(activity.Id))
//			return;

//		message.UserProperties ??= new Dictionary<string, object>();
//		message.UserProperties[MufloneActivitySource.TraceParentKey] = activity.Id;

//		if (!string.IsNullOrEmpty(activity.TraceStateString))
//		{
//			message.UserProperties[MufloneActivitySource.TraceStateKey] = activity.TraceStateString;
//		}
//	}

//	public static Guid GetCorrelationId(IMessage message)
//	{
//		message.UserProperties.TryGetValue(HeadersNames.CorrelationId, out var correlationId);
//		return correlationId != null ? Guid.Parse(correlationId.ToString()!) : Guid.Empty;
//	}


//	/// <summary>
//	/// Starts a consumer activity for an event.
//	/// The returned Activity must be disposed when the operation completes (use 'using' statement).
//	/// </summary>
//	/// <param name="event">The event being consumed</param>
//	/// <param name="activityName">Optional custom activity name (defaults to event type name)</param>
//	/// <returns>The started Activity, or null if activities are disabled. Dispose when operation completes.</returns>
//	public static Activity? StartConsumerActivity(this IEvent @event)
//	{
//		ArgumentNullException.ThrowIfNull(@event);
			
//		Activity? activity;

//		//using var activity = 

//		if (@event.UserProperties != null && TryExtractParentContext(@event.UserProperties, out var parentContext))
//		{
//			activity = ActivitySource.StartActivity(@event.GetType().Name, ActivityKind.Producer, parentContext);
//		}
//		else
//		{
//			activity = ActivitySource.StartActivity(@event.GetType().Name, ActivityKind.Producer);
//		}

		
//		if (activity != null)
//		{
//			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationPublish);
//			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, @event.GetType().Name);
//			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, @event.MessageId.ToString());
//			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageCorrelationId, GetCorrelationId(@event).ToString());
//		}

//		@event.InjectTraceContext();


//		return activity;
//	}

//	/// <summary>
//	/// Starts a producer activity for a command.
//	/// The returned Activity must be disposed when the operation completes (use 'using' statement).
//	/// </summary>
//	/// <param name="command">The command being produced</param>
//	/// <param name="activityName">Optional custom activity name (defaults to command type name)</param>
//	/// <returns>The started Activity, or null if activities are disabled. Dispose when operation completes.</returns>
//	public static Activity? StartProducerActivity(this ICommand command, string? activityName = null)
//	{
//		ArgumentNullException.ThrowIfNull(command);

//		var name = activityName ?? command.GetType().Name;
//		Activity? activity;

//		if (command.UserProperties != null && TryExtractParentContext(command.UserProperties, out var parentContext))
//		{
//			activity = MufloneActivitySource.Source.StartActivity(name, ActivityKind.Producer, parentContext);
//		}
//		else
//		{
//			activity = MufloneActivitySource.Source.StartActivity(name, ActivityKind.Producer);
//		}

//		if (activity != null)
//		{
//			activity.SetTag("messaging.operation", "publish");
//			activity.SetTag("messaging.message.type", command.GetType().Name);
//			activity.SetTag("messaging.message.id", command.MessageId.ToString());
//		}

//		return activity;
//	}

//	/// <summary>
//	/// Tries to extract parent ActivityContext from UserProperties using W3C Trace Context standard
//	/// </summary>
//	private static bool TryExtractParentContext(Dictionary<string, object> userProperties, out ActivityContext parentContext)
//	{
//		parentContext = default;

//		if (!userProperties.TryGetValue(MufloneActivitySource.TraceParentKey, out var traceparentObj))
//			return false;

//		var traceparent = traceparentObj as string;
//		if (string.IsNullOrEmpty(traceparent))
//			return false;

//		var tracestate = userProperties.TryGetValue(MufloneActivitySource.TraceStateKey, out var tracestateObj)
//										? tracestateObj as string
//										: null;

//		return ActivityContext.TryParse(traceparent, tracestate, out parentContext);
//	}
//}
