namespace Muflone.Messages.Commands;

public interface ICommandHandlerAsync : IMessageHandlerAsync
{
}

public interface ICommandHandlerAsync<TCommand> : ICommandHandlerAsync, IMessageHandlerAsync<TCommand> where TCommand : class, ICommand
{
}