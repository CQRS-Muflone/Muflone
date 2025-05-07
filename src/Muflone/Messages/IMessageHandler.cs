using System;

namespace Muflone.Messages;

public interface IMessageHandler : IDisposable
{
}

public interface IMessageHandler<TMessage> : IDisposable
    where TMessage : IMessage
{
    void Handle(TMessage message);
}