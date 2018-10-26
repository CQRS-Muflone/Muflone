using System;
using System.Threading.Tasks;
using Muflone.Messages;
using Muflone.Messages.Commands;

namespace Muflone
{
  public interface IServiceBus
  {
    Task Send<T>(T command) where T : class, ICommand;
    Task RegisterHandler<T>(Action<T> handler) where T : IMessage;
  }
}
