using Muflone.Messages.Commands;

namespace Muflone.Factories;

public interface ICommandHandlerFactory
{
    ICommandHandler<T> CreateCommandHandler<T>() where T : class, ICommand;
}