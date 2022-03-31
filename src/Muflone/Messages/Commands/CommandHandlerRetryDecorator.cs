﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Muflone.Messages.Commands
{
    public abstract class CommandHandlerRetryDecorator<TCommand> : ICommandHandlerAsync<TCommand> where TCommand : class, ICommand
    {
        private readonly ICommandHandlerAsync<TCommand> commandHandler;
        private readonly int retries;
        protected readonly ILogger Logger;

        protected CommandHandlerRetryDecorator(ICommandHandlerAsync<TCommand> commandHandler, ILoggerFactory loggerFactory, int retries = 3)
        {
            this.commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            this.retries = retries;
            Logger = loggerFactory?.CreateLogger(GetType()) ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var delay = 1000;
            for (var i = 0; ; i++)
            {
                try
                {
                    await commandHandler.HandleAsync(command, cancellationToken);
                    return;
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error on try {i} of {retries} for command {command.GetType()}: {e.Message} - Stacktrace: {e.StackTrace}");
                    delay *= 2;
                    await Task.Delay(delay);
                    if (i >= retries || IsPassThroughException(e))
                        throw;
                }
            }
        }

        private bool IsPassThroughException(Exception exception)
        {
            //TODO: If we want to implement some checks to let a specific exception throw without any retries

            return false;
        }

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

        ~CommandHandlerRetryDecorator()
        {
            Dispose(false);
        }
    }
}