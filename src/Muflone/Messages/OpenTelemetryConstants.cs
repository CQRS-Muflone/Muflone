namespace Muflone.Messages;

/// <summary>
/// Constants for OpenTelemetry instrumentation in message handlers.
/// </summary>
public static class OpenTelemetryConstants
{
	/// <summary>
	/// OpenTelemetry version used for ActivitySource.
	/// </summary>
	public const string Version = "1.0.0";

	/// <summary>
	/// W3C Trace Context traceparent header name.
	/// </summary>
	public const string TraceParentKey = "traceparent";

	/// <summary>
	/// W3C Trace Context tracestate header name.
	/// </summary>
	public const string TraceStateKey = "tracestate";

	/// <summary>
	/// Activity source names for different handler types.
	/// </summary>
	public static class ActivitySourceNames
	{
		public const string CommandHandler = "Muflone.CommandHandler";
		public const string DomainEventHandler = "Muflone.DomainEventHandler";
		public const string IntegrationEventHandler = "Muflone.IntegrationEventHandler";
		public const string ServiceBus = "Muflone.ServiceBus";
		public const string EventBus = "Muflone.EventBus";
		public const string Repository = "Muflone.Repository";
	}

	/// <summary>
	/// Standard OpenTelemetry semantic convention tags for messaging.
	/// </summary>
	public static class Tags
	{
		public const string MessagingOperation = "messaging.operation";
		public const string MessagingMessageType = "messaging.message.type";
		public const string MessagingMessageId = "messaging.message.id";
		public const string MessagingMessageCorrelationId = "messaging.message.correlation_id";
		public const string MessagingActivityId = "messaging.activity_id";
		public const string MessagingActivityTraceStateKey = "messaging.activity_tracestate_key";
		public const string DbSystem = "db.system";
		public const string DbOperation = "db.operation.name";
		public const string DbCollectionName = "db.collection.name";
	}

	/// <summary>
	/// Standard OpenTelemetry semantic convention tag values.
	/// </summary>
	public static class TagValues
	{
		public const string OperationPublish = "publish";
		public const string OperationConsume = "consume";
		public const string OperationSend = "send";
		public const string DbSystemEventStore = "eventstore";
	}

	/// <summary>
	/// Standard OpenTelemetry semantic convention event names.
	/// </summary>
	public static class Events
	{
		public const string Exception = "exception";
	}

	/// <summary>
	/// Standard OpenTelemetry semantic convention exception tags.
	/// </summary>
	public static class ExceptionTags
	{
		public const string Type = "exception.type";
		public const string Message = "exception.message";
		public const string Stacktrace = "exception.stacktrace";
	}
}
