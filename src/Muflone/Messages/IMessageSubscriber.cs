using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages;

public interface IMessageSubscriber
{
    /// <summary>
    /// Start the subscriber. This method has to be called after all the handlers have been registered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stop the subscriber
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Register commands/events handler. This type has to implement <see cref="IMessageHandler"/> 
    /// </summary>
    /// <param name="consumerType">The type be registered. The instance of this type will be retrieved using the ServiceProvider</param>
    /// <param name="configuration">Optional: handler configuration. Allows to customize the handler behaviour</param>
    void RegisterHandlers(Type consumerType, HandlerConfiguration? configuration = null);

    /// <summary>
    /// Register commands/events handler. This type has to implement <see cref="IMessageHandler"/> 
    /// </summary>
    /// <param name="consumer">The consumer instance to be registered.</param>
    /// <param name="configuration">Optional: handler configuration. Allows to customize the handler behaviour</param>
    void RegisterHandlers(IMessageHandlerAsync consumer, HandlerConfiguration? configuration = null);
}