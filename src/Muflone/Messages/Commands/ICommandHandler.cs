using System;

namespace Muflone.Messages.Commands;

public interface ICommandHandler : IDisposable
{
}

public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : class, ICommand
{
	void Handle(TCommand command);
}