using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.Persistence;

namespace Muflone.Messages;

public abstract class MessageSubscriberBase<TChannel>(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    : IMessageSubscriber
    where TChannel : class
{
    private readonly IServiceProvider? _serviceProvider = serviceProvider;
    private readonly List<HandlerSubscription<TChannel>> _consumersList = [];
    private readonly ILogger _logger = loggerFactory.CreateLogger(typeof(MessageSubscriberBase<TChannel>));

    public void RegisterHandlers(Type consumerType, HandlerConfiguration? configuration = null)
    {
        // if (!consumerType.IsAssignableTo(typeof(IMessageHandler)) || !consumerType.IsAssignableTo(typeof(IMessageHandlerAsync)))
        //     throw new NotSupportedException($"The consumer type must implement {nameof(IMessageHandler)} or {nameof(IMessageHandlerAsync)} interface");
        if (!consumerType.IsAssignableTo(typeof(IMessageHandlerAsync)))
            throw new NotSupportedException($"The consumer type must implement {nameof(IMessageHandlerAsync)} interface");

        RegisterCommandHandlers(consumerType, null, configuration);
        RegisterDomainEventHandlers(consumerType, null, configuration);
        RegisterIntegrationEventHandlers(consumerType, null, configuration);
    }

    public void RegisterHandlers(IMessageHandlerAsync consumer, HandlerConfiguration? configuration = null)
    {
        var consumerType = consumer.GetType();
        RegisterCommandHandlers(consumerType, consumer, configuration);
        RegisterDomainEventHandlers(consumerType, consumer, configuration);
        RegisterIntegrationEventHandlers(consumerType, consumer, configuration);
    }

    private void RegisterCommandHandlers(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null)
    {
        CommonRegisterHandlers(consumerType, consumerInstance, typeof(ICommandHandlerAsync),
            nameof(AddCommandConsumer),
            configuration);
    }

    private void RegisterDomainEventHandlers(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null)
    {
        CommonRegisterHandlers(consumerType, consumerInstance, typeof(IDomainEventHandlerAsync),
            nameof(AddDomainEventConsumers),
            configuration);
    }
    
    private void RegisterIntegrationEventHandlers(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null)
    {
        CommonRegisterHandlers(consumerType, consumerInstance, typeof(IIntegrationEventHandlerAsync),
            nameof(AddIntegrationEventConsumers),
            configuration);
    }

    private void CommonRegisterHandlers(Type consumerType, object? consumerInstance, Type handlerInterfaceType,
        string addConsumerMethodName,
        HandlerConfiguration? configuration = null)
    {
        var handlersInterfaces = consumerType.GetInterfaces()
            .Where(i => i.IsAssignableTo(handlerInterfaceType))
            .Where(i => i.IsGenericType);

        foreach (var @interface in handlersInterfaces)
        {
            var responseHandled = @interface.GetGenericArguments()[0];
            var methodInfo = GetType().GetMethod(addConsumerMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            var methodRef = methodInfo!.MakeGenericMethod(responseHandled);
            methodRef.Invoke(this, [consumerType, consumerInstance, configuration!]);
        }
    }

    protected void AddCommandConsumer<T>(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null)
        where T : Command
    {
        RegisterHandlerConsumer<T>(consumerType, consumerInstance, configuration, isCommand: true);
    }

    protected void AddDomainEventConsumers<T>(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null)
        where T : DomainEvent
    {
        RegisterHandlerConsumer<T>(consumerType, consumerInstance, configuration, isDomainEvent: true);
    }

    protected void AddIntegrationEventConsumers<T>(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null)
        where T : IntegrationEvent
    {
        RegisterHandlerConsumer<T>(consumerType, consumerInstance, configuration, isIntegrationEvent: true);
    }

    private void RegisterHandlerConsumer<T>(Type consumerType, object? consumerInstance,
        HandlerConfiguration? configuration = null,
        bool isCommand = false, bool isDomainEvent = false, bool isIntegrationEvent = false)
        where T : class, IMessage
    {
        var serializer = configuration?.CustomMessageSerializer ?? new Serializer();
        async Task Callback(string msg, CancellationToken cancellationToken)
        {
            var deserializedMessage = await DeserializeMessageAsync<T>(serializer, msg, cancellationToken);
            if (deserializedMessage is null) return;
            var handlerConsumerInstance = GetConsumerInstance<T>(consumerType, consumerInstance);
            try
            {
                await handlerConsumerInstance.HandleAsync(deserializedMessage, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error while processing message of type {typeof(T).Name}, consumer {handlerConsumerInstance.GetType().Name}. The message will be discarded",
                    ex);
            }
        }

        var eventName = typeof(T).Name;
        HandlerSubscription<TChannel> handlerSubscription = new(
            consumerType.Name,
            eventName,
            typeof(T),
            configuration,
            Callback,
            consumerInstance == null,
            isCommand, isDomainEvent, isIntegrationEvent);

        _consumersList.Add(handlerSubscription);
    }

    private async Task<T?> DeserializeMessageAsync<T>(ISerializer? customMessageSerializer, string msg,
        CancellationToken cancellationToken) where T : class, IMessage
    {
        try
        {
            var messageSerializer = customMessageSerializer ?? new Serializer();
            var message = await messageSerializer.DeserializeAsync<T>(msg, cancellationToken);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error while deserializing message of type {typeof(T).Name}. The message will be discarded", ex);
        }

        return null;
    }

    private IMessageHandlerAsync<T> GetConsumerInstance<T>(Type consumerType, object? consumerInstance)
        where T : class, IMessage
    {
        IMessageHandlerAsync<T>? handlerConsumerInstance;
        if (consumerInstance == null)
        {
            using var scope = _serviceProvider!.CreateScope();
            handlerConsumerInstance = (IMessageHandlerAsync<T>)scope.ServiceProvider!.GetRequiredService(consumerType);
        }
        else
        {
            handlerConsumerInstance = consumerInstance as IMessageHandlerAsync<T>;
        }

        if (handlerConsumerInstance is null)
            throw new Exception(
                $"Error while creating consumer instance of type {consumerType.Name} for event {typeof(T).Name}");
        
        return handlerConsumerInstance;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(_consumersList.Select(StartConsumerAsync).ToArray());
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var consumerSubscription in _consumersList)
        {
            StopChannelAsync(consumerSubscription);
        }

        return Task.CompletedTask;
    }

    private async Task StartConsumerAsync(HandlerSubscription<TChannel> handlerSubscription)
    {
        await InitChannelAsync(handlerSubscription);
        await InitSubscriptionAsync(handlerSubscription);
    }

    protected abstract Task StopChannelAsync(HandlerSubscription<TChannel> handlerSubscription);
    protected abstract Task InitChannelAsync(HandlerSubscription<TChannel> handlerSubscription);
    protected abstract Task InitSubscriptionAsync(HandlerSubscription<TChannel> handlerSubscription);
}