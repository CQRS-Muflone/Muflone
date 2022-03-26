using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Commands;

public abstract class CommandHandlerAsync<TCommand> : ICommandHandlerAsync<TCommand> where TCommand : class, ICommand
{
    public virtual Task HandleAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
    {
        return Task.CompletedTask;
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