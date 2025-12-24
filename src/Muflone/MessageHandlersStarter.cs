using Microsoft.Extensions.Hosting;
using Muflone.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone;

public class MessageHandlersStarter(IMessageSubscriber messageSubscriber)
        : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var types = GetAllRegisteredMessagesHandlerTypes();
        foreach (var type in types)
        {
            messageSubscriber.RegisterHandlers(type);
        }

        return messageSubscriber.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return messageSubscriber.StopAsync(cancellationToken);
    }

    private Type[] GetAllRegisteredMessagesHandlerTypes()
    {
        return MessageHandlerExtension.HandlersTypeReadOnlyList;
    }
}