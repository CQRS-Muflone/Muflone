using System;

namespace Muflone.Messages.Commands;

public interface ICommandFactory
{
	ICommandHandlerAsync CreateCommandHandlerAsync(Type handlerType);
}