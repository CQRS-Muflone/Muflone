using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Muflone.Messages;

public static class OpenTelemetryMessageHelpers
{
	public static Dictionary<string, object> CreateUserProperties()
	{
		var properties = new Dictionary<string, object>();
		var activity = Activity.Current;
		if (activity == null || string.IsNullOrEmpty(activity.Id))
			return properties;

		properties[OpenTelemetryConstants.TraceParentKey] = activity.Id;
		if (!string.IsNullOrEmpty(activity.TraceStateString))
			properties[OpenTelemetryConstants.TraceStateKey] = activity.TraceStateString;

		return properties;
	}

	public static void InjectTraceContext(IMessage message)
	{
		ArgumentNullException.ThrowIfNull(message);

		var activity = Activity.Current;
		if (activity == null || string.IsNullOrEmpty(activity.Id))
			return;

		if (message.UserProperties == null || !message.UserProperties.ContainsKey(OpenTelemetryConstants.TraceParentKey))
		{
			message.UserProperties = new Dictionary<string, object>();
			message.UserProperties[OpenTelemetryConstants.TraceParentKey] = activity.Id;
		}
		if (!string.IsNullOrEmpty(activity.TraceStateString))
			message.UserProperties[OpenTelemetryConstants.TraceStateKey] = activity.TraceStateString;
	}

	public static bool TryExtractParentContext(IMessage message, out ActivityContext parentContext)
	{
		parentContext = default;
		ArgumentNullException.ThrowIfNull(message);

		if (message.UserProperties == null)
			return false;

		if (!message.UserProperties.TryGetValue(OpenTelemetryConstants.TraceParentKey, out var traceparentObj))
			return false;

		var traceparent = traceparentObj as string;
		if (string.IsNullOrEmpty(traceparent))
			return false;

		var tracestate = message.UserProperties.TryGetValue(OpenTelemetryConstants.TraceStateKey, out var tracestateObj)
			? tracestateObj as string
			: null;

		return ActivityContext.TryParse(traceparent, tracestate, out parentContext);
	}
}
