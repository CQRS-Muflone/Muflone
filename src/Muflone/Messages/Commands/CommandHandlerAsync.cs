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

	//protected CommandHandlerAsync(ILoggerFactory loggerFactory)
	//{
	//    Logger = loggerFactory.CreateLogger(typeof(CommandHandlerAsync<TCommand>));
	//}

	//Had to rename it to avoid conflict with the interface method and implement OpenTelemetry
	protected abstract Task HandleInternalAsync(TCommand command, CancellationToken cancellationToken = default);

	public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(command);
		cancellationToken.ThrowIfCancellationRequested();

		using var activity = ActivitySource.StartActivity(typeof(TCommand).Name, ActivityKind.Consumer);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationConsume);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, typeof(TCommand).Name);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, command.MessageId);
		activity?.SetTag(OpenTelemetryConstants.Tags.MessagingMessageCorrelationId, GetCorrelationId(command));
		try
		{
			Logger.LogDebug("[Muflone.CommandHandlerAsync.HandleAsync] Handling command {CommandType} with MessageId {MessageId}", typeof(TCommand).Name, command.MessageId);
			await HandleInternalAsync(command, cancellationToken);
			Logger.LogDebug("[Muflone.CommandHandlerAsync.HandleAsync] Successfully handled command {CommandType} with MessageId {MessageId}", typeof(TCommand).Name, command.MessageId);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "[Muflone.CommandHandlerAsync.HandleAsync] Error handling command {CommandType} with MessageId {MessageId}", typeof(TCommand).Name, command.MessageId);
			activity?.AddException(ex);
			throw;
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