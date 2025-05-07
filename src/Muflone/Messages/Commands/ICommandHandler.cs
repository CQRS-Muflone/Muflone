namespace Muflone.Messages.Commands;

public interface ICommandHandler : IMessageHandler
{
}

public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : class, ICommand
{
}