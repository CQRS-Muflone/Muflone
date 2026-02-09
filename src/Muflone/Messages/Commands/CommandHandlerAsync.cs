using Microsoft.Extensions.Logging;
using Muflone.Persistence;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Commands;

public abstract class CommandHandlerAsync<TCommand> : ICommandHandlerAsync<TCommand> where TCommand : class, ICommand
{
	protected readonly IRepository Repository;
	protected readonly ILogger Logger;
	private static readonly ActivitySource ActivitySource = new(OpenTelemetryConstants.ActivitySourceNames.CommandHandler, OpenTelemetryConstants.Version);

	protected CommandHandlerAsync(IRepository repository, ILoggerFactory loggerFactory)
	{
		Repository = repository ?? throw new ArgumentNullException(nameof(repository));
		ArgumentNullException.ThrowIfNull(loggerFactory);
		Logger = loggerFactory.CreateLogger(typeof(CommandHandlerAsync<TCommand>));
	}

	public abstract Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);

	async Task IMessageHandlerAsync<TCommand>.HandleAsync(TCommand command, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(command);
		cancellationToken.ThrowIfCancellationRequested();

		var activityName = typeof(TCommand).Name;
		Activity? activity = OpenTelemetryMessageHelpers.TryExtractParentContext(command, out var parentContext)
			? ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext)
			: ActivitySource.StartActivity(activityName, ActivityKind.Consumer);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationConsume);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, typeof(TCommand).Name);
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, command.MessageId.ToString());
			activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageCorrelationId, GetCorrelationId(command).ToString());

			Logger.LogDebug("[Muflone.CommandHandlerAsync.HandleAsync] Handling command {CommandType} with MessageId {MessageId}", typeof(TCommand).Name, command.MessageId);
			await HandleAsync(command, cancellationToken);
			Logger.LogDebug("[Muflone.CommandHandlerAsync.HandleAsync] Successfully handled command {CommandType} with MessageId {MessageId}", typeof(TCommand).Name, command.MessageId);

			activity?.SetStatus(ActivityStatusCode.Ok);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "[Muflone.CommandHandlerAsync.HandleAsync] Error handling command {CommandType} with MessageId {MessageId}", typeof(TCommand).Name, command.MessageId);
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
		}
	}

	public Guid GetCorrelationId(TCommand command)
	{
		return MessageHelpers.GetCorrelationId(command);
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

	~CommandHandlerAsync()
	{
		Dispose(false);
	}

	#endregion
}
