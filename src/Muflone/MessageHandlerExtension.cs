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

    /// <param name="services"></param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Register a command handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddCommandHandler<T>() where T : ICommandHandlerAsync
        {
            return services.AddGenericHandler<T>();
        }

        /// <summary>
        /// Register a domain event handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddDomainEventHandler<T>() where T : IDomainEventHandlerAsync
        {
            return services.AddGenericHandler<T>();
        }

        /// <summary>
        /// Register an integration event handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddIntegrationEventHandler<T>() where T : IIntegrationEventHandlerAsync
        {
            return services.AddGenericHandler<T>();
        }

        /// <summary>
        /// Use this method to register a handler that is not related to commands or events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IServiceCollection AddGenericHandler<T>()
        {
            HandlersTypeList.Add(typeof(T));
            services.AddScoped(typeof(T));
            return services;
        }
    }
}