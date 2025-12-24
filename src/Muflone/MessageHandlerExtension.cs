using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using System;
using System.Collections.Generic;

namespace Muflone;

public static class MessageHandlerExtension
{
    private static readonly List<Type> HandlersTypeList = [];
    public static Type[] HandlersTypeReadOnlyList => HandlersTypeList.ToArray();

    /// <summary>
    /// Register a command handler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCommandHandler<T>(this IServiceCollection services) where T : ICommandHandlerAsync
    {
        return AddGenericHandler<T>(services);
    }

    /// <summary>
    /// Register a domain event handler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDomainEventHandler<T>(this IServiceCollection services) where T : IDomainEventHandlerAsync
    {
        return AddGenericHandler<T>(services);
    }

    /// <summary>
    /// Register an integration event handler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddIntegrationEventHandler<T>(this IServiceCollection services) where T : IIntegrationEventHandlerAsync
    {
        return AddGenericHandler<T>(services);
    }

    /// <summary>
    /// Use this method to register a handler that is not related to commands or events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddGenericHandler<T>(this IServiceCollection services)
    {
        HandlersTypeList.Add(typeof(T));
        services.AddScoped(typeof(T));
        return services;
    }
}