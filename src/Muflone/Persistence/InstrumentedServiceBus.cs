using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.Messages.Commands;
using OpenTelemetry.Trace;

namespace Muflone.Persistence;

/// <summary>
/// Decorator that wraps an IServiceBus implementation with OpenTelemetry producer spans.
/// </summary>
public sealed class InstrumentedServiceBus : IServiceBus
{
	private readonly IServiceBus _inner;
	private static readonly ActivitySource ActivitySource = new(
		OpenTelemetryConstants.ActivitySourceNames.ServiceBus,
		OpenTelemetryConstants.Version);

	public InstrumentedServiceBus(IServiceBus inner)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
	}

	public async Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
	{
		var activityName = $"{typeof(T).Name} send";
		Activity? activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationSend);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, typeof(T).Name);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, command.MessageId.ToString());

			// Re-inject trace context so the command carries the producer span's trace context
			// for the consumer handler to pick up as its parent
			OpenTelemetryMessageHelpers.InjectTraceContext(command);

			await _inner.SendAsync(command, cancellationToken);

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
