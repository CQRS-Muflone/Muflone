using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages;

public interface IMessageHandlerAsync : IDisposable
{
}

public interface IMessageHandlerAsync<TMessage> : IDisposable
    where TMessage : IMessage
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}