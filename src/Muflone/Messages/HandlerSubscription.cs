using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages;

public sealed class HandlerSubscription<T>(
    string consumerTypeName,
    string eventTypeName,
    HandlerConfiguration? configuration,
    Func<string, CancellationToken, Task> messageAsync,
    bool isSingletonHandler,
    bool isCommandHandler,
    bool isDomainEventHandler,
    bool isIntegrationEventHandler)
    where T : class
{
    public string HandlerSubscriptionId { get; } = configuration?.InstanceId ?? Guid.NewGuid().ToString();

    public string EventTypeName { get; } = eventTypeName;
    public HandlerConfiguration? Configuration { get; } = configuration;
    public bool IsCommandHandler { get; } = isCommandHandler;
    public bool IsDomainEventHandler { get; } = isDomainEventHandler;
    public bool IsIntegrationEventHandler { get; } = isIntegrationEventHandler;

    public Func<string, CancellationToken, Task> MessageAsync { get; } = messageAsync;
    public bool IsSingletonHandler { get; } = isSingletonHandler;
    public T? Channel { get; set; }
}