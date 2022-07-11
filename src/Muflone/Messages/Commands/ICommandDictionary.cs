using System.Collections.Generic;

namespace Muflone.Messages.Commands;

public interface ICommandDictionary
{
	IDictionary<ICommand, ICommandHandler<ICommand>> Observers { get; }

	void RegisterCommand<TCommand, TImplementation>() where TCommand : class, ICommand
		where TImplementation : class, ICommandHandler<TCommand>;
}