namespace Muflone.Messages.Commands;

public interface ICommandHandlerAsync : IMessageHandlerAsync
{
}

public interface ICommandHandlerAsync<TCommand> : IMessageHandlerAsync<TCommand> where TCommand : class, ICommand
{
}