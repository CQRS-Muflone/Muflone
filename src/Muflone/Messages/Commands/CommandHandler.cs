using Microsoft.Extensions.Logging;
using Muflone.Persistence;
using System;

namespace Muflone.Messages.Commands;

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : class, ICommand
{
  protected readonly IRepository Repository;
  protected readonly ILogger Logger;

  protected CommandHandler(IRepository repository, ILoggerFactory loggerFactory)
  {
    Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    Logger = loggerFactory.CreateLogger(GetType());
  }

  public abstract void Handle(TCommand command);


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

  ~CommandHandler()
  {
    Dispose(false);
  }

  #endregion
}