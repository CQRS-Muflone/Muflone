using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Muflone;

public static class MessageHandlerExtension
{
    private static readonly List<Type> HandlersTypeList = [];
    public static Type[] HandlersTypeReadOnlyList => HandlersTypeList.ToArray();

    public static IServiceCollection AddMessageHandler<T>(this IServiceCollection services)
    {
        HandlersTypeList.Add(typeof(T));
        services.AddScoped(typeof(T));
        return services;
    }
}