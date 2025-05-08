namespace Muflone.Messages.Commands;

public interface ICommandHandlerAsync : IMessageHandlerAsync
{
}

public interface ICommandHandlerAsync<TCommand> : ICommandHandler, IMessageHandlerAsync<TCommand> where TCommand : class, ICommand
{
}