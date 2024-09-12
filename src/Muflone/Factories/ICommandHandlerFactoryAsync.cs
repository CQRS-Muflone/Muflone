using Muflone.Messages.Commands;

namespace Muflone.Factories;

public interface ICommandHandlerFactoryAsync
{
	ICommandHandlerAsync<T> CreateCommandHandlerAsync<T>() where T : class, ICommand;
}