using System;
using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.Messages.Commands;

namespace Muflone;

public interface IServiceBus
{
	Task SendAsync<T>(T command) where T : class, ICommand;
	Task RegisterHandlerAsync<T>(Action<T> handler) where T : IMessage;
}